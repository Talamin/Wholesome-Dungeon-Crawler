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
using static wManager.Wow.Class.Npc;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class RegroupStep : Step
    {
        private RegroupModel _regroupModel;
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private Timer _readyCheckTimer = new Timer();
        private int _foodMin;
        private int _drinkMin;
        private bool _drinkAllowed;
        private bool _imPartyLeader;
        private bool _receivedChatSystemReady;
        private RegroupRaidIcons _stepIcon;

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }
        public Vector3 RegroupSpot { get; private set; }

        public RegroupStep(RegroupModel regroupModel, IEntityCache entityCache, IPartyChatManager partyChatManager) : base(regroupModel.CompleteCondition)
        {
            _partyChatManager = partyChatManager;
            _regroupModel = regroupModel;
            _entityCache = entityCache;
            Name = regroupModel.Name;
            StepFaction = regroupModel.StepFaction;
            StepRole = regroupModel.StepRole;
            RegroupSpot = regroupModel.RegroupSpot;
            _foodMin = wManager.wManagerSetting.CurrentSetting.FoodPercent;
            _drinkMin = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
            _drinkAllowed = wManager.wManagerSetting.CurrentSetting.RestingMana;
            Lua.LuaDoString($"SetRaidTarget('player', 0)");
            PreEvaluationPass = EvaluateFactionCompletion();
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public enum RegroupRaidIcons
        {
            STAR = 1,
            DIAMOND = 3,
            TRIANGLE = 4
        }

        public void SetRaidIcon(RegroupRaidIcons icon)
        {
            _stepIcon = icon;
        }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

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
                && _entityCache.Me.PositionWT.DistanceTo(RegroupSpot) > 5f)
            {
                Logger.LogOnce($"[{_regroupModel.Name}] Moving to regroup spot location");
                GoToTask.ToPosition(RegroupSpot, 0.5f);
                IsCompleted = false;
                return;
            }

            // Move to the exact regroup spot
            if (!MovementManager.InMovement
                && !MovementManager.InMoveTo
                && _entityCache.Me.PositionWT.DistanceTo(RegroupSpot) > 1f)
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
                MarkAsCompleted();
                return;
            }

            // Check if everyone is here
            if (_entityCache.ListGroupMember.Length != _entityCache.ListPartyMemberNames.Count
                || _entityCache.ListGroupMember.Any(member => member.PositionWT.DistanceTo(RegroupSpot) > 8f)
                || _entityCache.Me.PositionWT.DistanceTo(RegroupSpot) > 8f)
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

            if (!EvaluateCompleteCondition())
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
                    Lua.LuaDoString($"SetRaidTarget('player', {(int)_stepIcon})");
                    Task.Run(async delegate
                    {
                        int delay = 1000;
                        int maxWaitMs = delay * 60;
                        while (maxWaitMs > 0 && _entityCache.ListGroupMember.Any(m => m.PositionWT.DistanceTo(RegroupSpot) < 5f))
                        {
                            maxWaitMs -= delay;
                            await Task.Delay(delay);
                        }
                        Lua.LuaDoString($"SetRaidTarget('player', 0)");
                    });
                    _receivedChatSystemReady = false;
                    CompleteStep();
                    return;
                }

                // We need to initiate the check
                if (luaTimeRemaining <= 0)
                {
                    Toolbox.DoGroupReadyCheck();
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
                    Toolbox.AnswerYesReadyCHeck();
                }

                if (Toolbox.MemberHasRaidTarget((int)_stepIcon))
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
            Thread.Sleep(2000);
            Logger.Log("Everyone is ready");
            _partyChatManager.SetRegroupStep(null);
            MarkAsCompleted();
        }
    }
}
