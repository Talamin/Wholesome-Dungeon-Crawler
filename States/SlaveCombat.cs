using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class SlaveCombat : State
    {
        public override string DisplayName => "InFight";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public SlaveCombat(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
        }

        private IWoWUnit Target;

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || !_cache.IsInInstance
                    || Fight.InFight
                    || _entityCache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.Target.Dead)
                {
                    Interact.ClearTarget();
                }

                Target = null;

                // Defend tank
                if (_entityCache.TankUnit != null)
                {
                    IWoWUnit attackingTank = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid)
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(_entityCache.TankUnit.PositionWithoutType))
                        .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                        .FirstOrDefault();
                    if (attackingTank != null)
                    {
                        Target = attackingTank;
                        Logger.Log($"SlaveCombat: Target attacking tank {Target.Name}, start defending");
                        return true;
                    }
                }

                // Defend players when the tank is dead, out of OM, or has no target
                IWoWUnit attackingGroup = _entityCache.EnemiesAttackingGroup
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType))
                    .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                    .FirstOrDefault();
                if (attackingGroup != null)
                {
                    Target = attackingGroup;
                    Logger.Log($"SlaveCombat: Target attacking player {Target.Name}, start defending");
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();
            Fight.StopFight();
            Logger.Log("Start Fight with: " + Target.Guid + " Slave Combat State");
            ObjectManager.Me.Target = Target.Guid;
            Fight.StartFight(Target.Guid, false);
        }
    }
}
