/*using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class LeaveDungeon : State
    {
        public override string DisplayName => "Leaving Dungeon";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private Timer _leaveTimer = null;
        private readonly int _timerLength = 10000;

        public LeaveDungeon(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || _cache.LootRollShow)
                {
                    return false;
                }

                if (_profileManager.CurrentDungeonProfile.ProfileIsCompleted
                    && _leaveTimer == null)
                {
                    Logger.Log($"Leaving dungeon in {_timerLength / 1000} seconds");
                    _leaveTimer = new Timer(_timerLength);
                }

                return _leaveTimer != null;
            }
        }


        public override void Run()
        {
            if (_leaveTimer.IsReady)
            {
                Logger.Log($"Profile is finished, leaving.");
                _profileManager.UnloadCurrentProfile();
                Toolbox.LeaveDungeonAndGroup();
                _leaveTimer = null;
            }
        }
    }
}
*/