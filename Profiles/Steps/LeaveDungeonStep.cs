using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class LeaveDungeonStep : Step
    {
        private LeaveDungeonModel _leaveDungeonModel;
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private readonly IProfileManager _profileManager;
        public override string Name { get; }
        private Timer _readyCheckTimer = new Timer();
        private int _foodMin;
        private int _drinkMin;
        private bool _drinkAllowed;
        private readonly int _readyTargetIndex = 2;
        private bool _imPartyLeader;
        private bool _receivedChatSystemReady;

        public LeaveDungeonStep(LeaveDungeonModel leaveDungeonModel, 
            IEntityCache entityCache,
            IPartyChatManager partyChatManager,
            IProfileManager profileManager) : base(leaveDungeonModel.CompleteCondition)
        {
            _profileManager = profileManager;
            _partyChatManager = partyChatManager;
            _leaveDungeonModel = leaveDungeonModel;
            _entityCache = entityCache;
            Name = leaveDungeonModel.Name;
            _foodMin = wManager.wManagerSetting.CurrentSetting.FoodPercent;
            _drinkMin = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
            _drinkAllowed = wManager.wManagerSetting.CurrentSetting.RestingMana;
            Lua.LuaDoString($"SetRaidTarget('player', 0)");
        }

        public override void Run()
        {
            if (_entityCache.Me.IsDead || _entityCache.EnemiesAttackingGroup.Length > 0)
            {
                IsCompleted = false;
                return;
            }

            MovementManager.StopMove();
            _partyChatManager.SetLeaveDungeonStep(this);

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

            // Auto complete if running alone
            if (_entityCache.ListPartyMemberNames.Count == 0)
            {
                Logger.LogOnce($"Not in a group, leaving");
                CompleteStep();
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
            Logger.Log("Everyone is ready. Leaving dungeon.");
            _partyChatManager.SetLeaveDungeonStep(null);
            _profileManager.UnloadCurrentProfile();
            Thread.Sleep(8000);
            Toolbox.LeaveDungeonAndGroup();
        }
    }
}
