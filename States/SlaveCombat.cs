using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class SlaveCombat : State
    {
        public override string DisplayName => "InFight";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public SlaveCombat(
            ICache iCache, 
            IEntityCache EntityCache,
            IProfileManager profileManager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profileManager;
        }

        private IWoWUnit Target;

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.IsValid
                    || !_cache.IsInInstance
                    || Fight.InFight
                    || _entityCache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.Target.IsDead)
                {
                    Interact.ClearTarget();
                }

                // Block state if pulling to safe spot
                PullToSafeSpotStep pullToSafeSpotStep = null;
                if (_profileManager.CurrentDungeonProfile?.CurrentStep != null
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                {
                    return false;
                    //pullToSafeSpotStep = _profileManager.CurrentDungeonProfile.CurrentStep as PullToSafeSpotStep;
                }

                Target = null;

                // Defend tank
                if (_entityCache.TankUnit != null)
                {
                    IWoWUnit attackingTank = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid
                            && (pullToSafeSpotStep == null || pullToSafeSpotStep.PositionInSafeSpotFightRange(unit.PositionWithoutType)))
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(_entityCache.TankUnit.PositionWithoutType))
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
                    .Where(unit => pullToSafeSpotStep == null || pullToSafeSpotStep.PositionInSafeSpotFightRange(unit.PositionWithoutType))
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType))
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
            ObjectManager.Me.Target = Target.Guid;
            Fight.StartFight(Target.Guid, false);
        }
    }
}
