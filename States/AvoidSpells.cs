using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class AvoidSpells : State
    {
        public override string DisplayName => "Avoiding Spell";

        private readonly IAvoidAOEManager _avoidSpellManager;
        private readonly IEntityCache _entityCache;
        private readonly ICache _cache;

        public AvoidSpells(
            IEntityCache entityCache,
            IAvoidAOEManager avoidSpellManager,
            ICache cache)
        {
            _entityCache = entityCache;
            _avoidSpellManager = avoidSpellManager;
            _cache = cache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_cache.IsInInstance
                    || _entityCache.Me.IsDead
                    || !_avoidSpellManager.ShouldReposition)
                {
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {
            MovementManager.Go(_avoidSpellManager.GetEscapePath, false);
        }
    }
}
