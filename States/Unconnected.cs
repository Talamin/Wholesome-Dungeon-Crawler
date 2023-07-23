using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class Unconnected : State, IState
    {
        public override string DisplayName => "Forced Stop";
        private readonly IEntityCache _entityCache;

        public Unconnected(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                return !Conditions.InGameAndConnectedAndProductStarted
                    || !_entityCache.Me.IsValid;
            }
        }

        public override void Run()
        {
            Logger.LogOnce($"Waiting for reconnection", true);
            MovementManager.StopMove();
            Thread.Sleep(1000);
        }
    }
}
