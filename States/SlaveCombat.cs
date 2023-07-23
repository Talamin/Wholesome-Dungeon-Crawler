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
        public override string DisplayName => "Follower Combat";

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
                if (!_cache.IsInInstance
                    || _entityCache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.Target.IsDead)
                {
                    Interact.ClearTarget();
                }

                // Block state if pulling to safe spot
                if (_profileManager.ProfileIsRunning
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                {
                    return false;
                }

                Target = null;

                // Defend tank
                if (_entityCache.TankUnit != null)
                {
                    IWoWUnit attackingTank = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid)
                        .OrderBy(unit => unit.PositionWT.DistanceTo(_entityCache.TankUnit.PositionWT))
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
                    .OrderBy(unit => unit.PositionWT.DistanceTo(_entityCache.Me.PositionWT))
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
