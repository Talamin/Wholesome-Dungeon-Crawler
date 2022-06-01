using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class SlaveCombat : State
    {
        public override string DisplayName => "InFight";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public SlaveCombat(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
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
                    || _cache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.Target.Dead)
                {
                    Interact.ClearTarget();
                }

                Target = null;

                // Defend players when the tank is dead, out of OM, or has no target
                if (_entityCache.TankUnit == null || _entityCache.TankUnit.Dead || _entityCache.TankUnit.TargetGuid == 0)
                {
                    IWoWUnit _attackingPlayer = AttackingPlayer();
                    if (_attackingPlayer != null)
                    {
                        Target = _attackingPlayer;
                        Logger.Log($"Target attacking Player: {Target.Name} , start defending");
                        return true;
                    }
                }

                // Defend tank
                if (_entityCache.TankUnit != null)
                {
                    IWoWUnit _attackingTank = AttackingTank(_entityCache.TankUnit);
                    if (_attackingTank != null && _entityCache.Me.TargetGuid == 0)
                    {
                        Target = _attackingTank;
                        Logger.Log($"Target attacking Tank: {Target.Name} , start defending");
                        return true;
                    }
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
            //Fight.CurrentTarget = Target.WowUnit;
            Fight.StartFight(Target.Guid, false);
        }


        private IWoWUnit AttackingTank(IWoWUnit tank) => FindClosestUnit(unit =>
                unit.TargetGuid == tank.Guid, 
                tank.PositionWithoutType);

        private IWoWUnit AttackingPlayer() => FindClosestUnit(unit => unit.IsAttackingGroup || unit.IsAttackingMe);

        private IWoWUnit FindClosestUnit(Func<IWoWUnit, bool> predicate, Vector3 referencePosition = null)
        {
            IWoWUnit foundUnit = null;
            var distanceToUnit = float.MaxValue;

            Vector3 position = referencePosition != null ? referencePosition : _entityCache.Me.PositionWithoutType;

            foreach (IWoWUnit unit in _entityCache.EnemyUnitsList)
            {
                if (!predicate(unit)) continue;

                if (foundUnit == null)
                {
                    distanceToUnit = WTPathFinder.CalculatePathTotalDistance(position, unit.PositionWithoutType);
                    foundUnit = unit;
                }
                else
                {
                    float currentDistanceToUnit = WTPathFinder.CalculatePathTotalDistance(position, unit.PositionWithoutType);
                    if (currentDistanceToUnit < distanceToUnit)
                    {
                        foundUnit = unit;
                        distanceToUnit = currentDistanceToUnit;
                    }
                }
            }
            return foundUnit;
        }
    }
}
