using robotManager.Helpful;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class RegroupStep : Step
    {
        private RegroupModel _regroupModel;
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        public override string Name { get; }
        public Vector3 RegroupSpot { get; private set; }
        private Timer _readyCheckTimer = new Timer();
        private int _foodMin;
        private int _drinkMin;
        private bool _drinkAllowed;
        private readonly int _readyTargetIndex = 1;
        private bool _imPartyLeader;
        private bool _receivedChatSystemReady;

        public RegroupStep(RegroupModel regroupModel, IEntityCache entityCache, IPartyChatManager partyChatManager)
        {
            _partyChatManager = partyChatManager;
            _regroupModel = regroupModel;
            _entityCache = entityCache;
            Name = regroupModel.Name;
            RegroupSpot = regroupModel.RegroupSpot;
            _foodMin = wManager.wManagerSetting.CurrentSetting.FoodPercent;
            _drinkMin = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
            _drinkAllowed = wManager.wManagerSetting.CurrentSetting.RestingMana;
            Lua.LuaDoString($"SetRaidTarget('player', 0)");
        }

        public override void Run()
        {
            _partyChatManager.SetRegroupStep(this);

            if (_entityCache.Me.IsDead || _entityCache.EnemiesAttackingGroup.Length > 0)
            {
                IsCompleted = false;
                return;
            }

            // Ensure we interrupt any unwanted move
            if (MovementManager.InMovement && MovementManager.CurrentPath.Last() != RegroupSpot)
            {
                Logger.LogOnce($"[{_regroupModel.Name}] Stopping move");
                MovementManager.StopMove();
            }
            if (MovementManager.InMoveTo && MovementManager.CurrentMoveTo != RegroupSpot)
            {
                Logger.LogOnce($"[{_regroupModel.Name}] Stopping move to");
                MovementManager.StopMoveTo();
            }

            // Move to regroup spot location
            if (!MovementManager.InMovement
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 5f)
            {
                Logger.LogOnce($"[{_regroupModel.Name}] Moving to regroup spot location");
                GoToTask.ToPosition(RegroupSpot, 0.5f);
                IsCompleted = false;
                return;
            }

            // Move to the exact regroup spot
            if (!MovementManager.InMovement
                && !MovementManager.InMoveTo
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 1f)
            {
                Logger.LogOnce($"[{_regroupModel.Name}] Moving precisely to regroup spot");
                MovementManager.MoveTo(RegroupSpot);
                IsCompleted = false;
                return;
            }

            // Auto complete if running alone
            if (_entityCache.ListPartyMemberNames.Count == 0)
            {
                Logger.LogOnce($"Not in a group, skipping");
                IsCompleted = true;
                return;
            }

            // Check if everyone is here
            if (_entityCache.ListGroupMember.Length != _entityCache.ListPartyMemberNames.Count
                || _entityCache.ListGroupMember.Any(member => member.PositionWithoutType.DistanceTo(RegroupSpot) > 8f)
                || _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 8f)
            {
                Logger.LogOnce($"Waiting for the team to regroup.");
                IsCompleted = false;
                return;
            }

            // Check for regen conditions
            if (_drinkAllowed && _entityCache.Me.Mana > 0 && _entityCache.Me.ManaPercent < _drinkMin)
            {
                Logger.LogOnce($"Skipping ready check vote until mana is restored");
                IsCompleted = false;
                return;
            }

            if (_entityCache.Me.HealthPercent < _foodMin)
            {
                Logger.LogOnce($"Skipping ready check vote until health is restored");
                IsCompleted = false;
                return;
            }

            if (!EvaluateCompleteCondition(_regroupModel.CompleteCondition))
            {
                IsCompleted = false;
                return;
            }

            _imPartyLeader = Lua.LuaDoString<bool>("return IsPartyLeader() == 1;");
            int luaTimeRemaining = Lua.LuaDoString<int>("return GetReadyCheckTimeLeft();");
            bool imReady = Lua.LuaDoString<bool>($"return GetReadyCheckStatus('{_entityCache.Me.Name}') == 'ready';");

            // If you're the party leader, the LUA timer goes back to 0 when a ready check is finished
            // If you're not the party leader, the countdown continues even after a ready check ends

            if (luaTimeRemaining > _readyCheckTimer.TimeLeft() / 1000)
            {
                _readyCheckTimer.Reset(luaTimeRemaining * 1000);
            }

            Thread.Sleep(1000);
            // Party leader logic
            if (_imPartyLeader)
            {
                if (_receivedChatSystemReady)
                {
                    Lua.LuaDoString($"SetRaidTarget('player', {_readyTargetIndex})");
                    Task.Run(async delegate
                    {
                        await Task.Delay(30000);
                        Lua.LuaDoString($"SetRaidTarget('player', 0)");
                    });
                    _receivedChatSystemReady = false;
                    CompleteStep();
                    return;
                }

                // We need to initiate the check
                if (luaTimeRemaining <= 0)
                {
                    Logger.Log($"No ready check in progress. Initiating.");
                    Lua.LuaDoString($"SetRaidTarget('player', 0)");
                    Lua.LuaDoString("DoReadyCheck();");
                    Thread.Sleep(1000);
                    return;
                }

                IsCompleted = false;
                return;
            }
            else
            // Party follower logic
            {
                if (!imReady && _readyCheckTimer.TimeLeft() > 3000)
                {
                    Logger.LogOnce($"Answering yes to ready check.");
                    Lua.LuaDoString("ReadyCheckFrameYesButton:Click();");
                    Lua.LuaDoString("ConfirmReadyCheck(true);");
                }

                if (Toolbox.MemberHasRaidTarget(_readyTargetIndex))
                {
                    CompleteStep();
                    return;
                }

                IsCompleted = false;
                return;
            }
        }

        public void PartyReadyReceived()
        {
            Logger.LogOnce($"Received party ready system message");
            _receivedChatSystemReady = true;
        }

        private void CompleteStep()
        {
            if (!_entityCache.IAmTank)
            {
                Thread.Sleep(1000);
            }
            Logger.Log("Everyone is ready");
            _partyChatManager.SetRegroupStep(null);
            IsCompleted = true;
        }
    }
}
