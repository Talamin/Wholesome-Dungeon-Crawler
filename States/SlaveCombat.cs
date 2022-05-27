using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class SlaveCombat : State, ICycleable
    {
        public override string DisplayName => "InFight";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private IWoWUnit Target;

        public SlaveCombat(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || !_cache.IsInInstance
                    || _entityCache.Me.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                {
                    return false;
                }

                if (_entityCache.Target.Dead)
                {
                    Interact.ClearTarget();
                }

                if(!Fight.InFight)
                {
                    if (AttackingTank(_entityCache.TankUnit) != null)
                    {
                        Target = AttackingTank(_entityCache.TankUnit);
                        Logger.Log($"Attacking: {Target.Name} is attacking Tank, switching");
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
            Fight.StartFight(Target.Guid, false);
        }

        public void Targetswitcher(WoWUnit target, CancelEventArgs cancable)
        {
            IWoWUnit fleeUnit = FleeingUnit(_entityCache.TankUnit);
            if (fleeUnit != null && _entityCache.Me.TargetGuid != fleeUnit.Guid)
            {
                Target = FleeingUnit(_entityCache.TankUnit);
                Logger.Log($"Attacking: {Target.Name} is attacking Fleeing, switching");
                SwitchedTargetFight(Target);
            }
            //Check to AssistTank
            if (AssistTank(_entityCache.TankUnit) != null && _entityCache.Me.TargetGuid == 0)
            {
                Target = AssistTank(_entityCache.TankUnit);
                Logger.Log($"Attacking: {Target.Name} is attacking Tank, switching");
                SwitchedTargetFight(Target);
            }

            //check to Assist any  Groupmember if Tank don´t get the aggro
            if (AssistGroup(_entityCache.TankUnit) != null && _entityCache.Me.TargetGuid == 0)
            {
                Target = AssistTank(_entityCache.TankUnit);
                Logger.Log($"Attacking: {Target.Name} is attacking Groupmember, switching");
                SwitchedTargetFight(Target);
            }
        }

        private void SwitchedTargetFight(IWoWUnit target)
        {
            MovementManager.StopMove();
            Fight.StopFight();
            Fight.StartFight(target.Guid, false);
        }


        private IWoWUnit AttackingTank(IWoWUnit tank)
        {
            IWoWUnit Unit = FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.TargetGuid == tank.Guid
            && !unit.Dead, tank.PositionWithoutType);
            return Unit;
        }

        private IWoWUnit FleeingUnit(IWoWUnit Tank)
        {
            IWoWUnit Unit = FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.Fleeing
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType);
            return Unit;
        }

        private IWoWUnit AssistTank(IWoWUnit Tank)
        {
            IWoWUnit Unit = FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.TargetGuid == Tank.Guid
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType);
            return Unit;
        }

        private IWoWUnit AssistGroup(IWoWUnit Tank)
        {
            IWoWUnit Unit = FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.TargetGuid != Tank.Guid
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType);
            return Unit;
        }


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
                    distanceToUnit = position.DistanceTo(unit.PositionWithoutType);
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

        public void Initialize()
        {
            wManager.Events.FightEvents.OnFightLoop += Targetswitcher;
        }

        public void Dispose()
        {
            wManager.Events.FightEvents.OnFightLoop -= Targetswitcher;
        }
    }
}
