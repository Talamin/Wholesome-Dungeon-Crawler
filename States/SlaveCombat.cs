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
        private ICachedWoWUnit _foundtarget;

        public SlaveCombat(
            ICache iCache, 
            IEntityCache EntityCache,
            IProfileManager profileManager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profileManager;
        }

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
                    //Logger.Log($"{_entityCache.Target.Name} is dead, dropping target");

                    Interact.ClearTarget();
                }

                // Block state if pulling to safe spot or MAP with ignore fights
                if (_profileManager.ProfileIsRunning)
                {
                    if (_profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                        return false;

                    if (_profileManager.CurrentDungeonProfile.CurrentStep is MoveAlongPathStep mapStep
                        && mapStep.IgnoreFightsDuringPath)
                        return false;
                }

                _foundtarget = null;

                // Defend tank
                if (_entityCache.TankUnit != null)
                {
                    ICachedWoWUnit attackingTank = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid)
                        .OrderBy(unit => unit.PositionWT.DistanceTo(_entityCache.TankUnit.PositionWT))
                        .FirstOrDefault();
                    if (attackingTank != null)
                    {
                        _foundtarget = attackingTank;
                        Logger.Log($"SlaveCombat: Target attacking tank {_foundtarget.Name}, start defending");
                        return true;
                    }
                }

                // Defend players when the tank is dead, out of OM, or has no target
                ICachedWoWUnit attackingGroup = _entityCache.EnemiesAttackingGroup
                    .OrderBy(unit => unit.PositionWT.DistanceTo(_entityCache.Me.PositionWT))
                    .FirstOrDefault();
                if (attackingGroup != null)
                {
                    _foundtarget = attackingGroup;
                    Logger.Log($"SlaveCombat: Target attacking player {_foundtarget.Name}, start defending");
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();
            //Fight.StopFight();
            //ObjectManager.Me.Target = _foundtarget.Guid;
            Fight.StartFight(_foundtarget.Guid, false);
        }
    }
}
