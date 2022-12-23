using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class LoadingScreenLock : State, IState
    {
        public override string DisplayName => "Loading screen lock";
        private readonly ICache _cache;

        public LoadingScreenLock(ICache iCache)
        {
            _cache = iCache;
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
            Thread.Sleep(500);
        }
    }
}
