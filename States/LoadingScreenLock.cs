using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class LoadingScreenLock : State, IState
    {
        public override string DisplayName => "Loading screen lock";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private Timer _lockTimer = null;

        public LoadingScreenLock(ICache iCache, IEntityCache entityCache)
        {
            _cache = iCache;
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected)
                {
                    return false;
                }

                return _cache.InLoadingScreen;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();

            if (_lockTimer == null)
            {
                _lockTimer = new Timer(5000);
            }

            if (_lockTimer.IsReady)
            {
                _entityCache.CacheGroupMembers("Loading screen lock end");
                _cache.ResetLoadingScreenLock();
                _lockTimer = null;
            }
        }
    }
}
