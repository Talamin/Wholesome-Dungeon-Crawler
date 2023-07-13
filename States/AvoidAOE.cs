using robotManager.FiniteStateMachine;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class AvoidAOE : State
    {
        public override string DisplayName => "Escaping from AOE damage";

        private readonly IAvoidAOEManager _avoidAOEManager;
        private readonly IEntityCache _entityCache;
        private readonly ICache _cache;

        public AvoidAOE(
            IEntityCache entityCache,
            IAvoidAOEManager avoidAOEManager,
            ICache cache)
        {
            _entityCache = entityCache;
            _avoidAOEManager = avoidAOEManager;
            _cache = cache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_cache.IsInInstance
                    || _entityCache.Me.IsDead
                    || !_entityCache.Me.IsValid
                    || !_avoidAOEManager.ShouldReposition)
                {
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {
            MovementManager.Go(_avoidAOEManager.GetEscapePath, false);
        }
    }
}
