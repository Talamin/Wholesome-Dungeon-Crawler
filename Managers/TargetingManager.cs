﻿using robotManager.Helpful;
using System.ComponentModel;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    class TargetingManager : ITargetingManager
    {

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public TargetingManager(IEntityCache entityCache, ICache cache)
        {
            _entityCache = entityCache;
            _cache = cache;
        }

        private IWoWUnit Target;

        public void Targetswitcher(WoWUnit target, CancelEventArgs cancable)
        {
            if (_entityCache.Target.Dead)
            {
                Interact.ClearTarget();
            }
            Target = null;

            if (_entityCache.IAmTank)
            {
                IWoWUnit currentTarget = _entityCache.Target;
                if (currentTarget.IsAttackingMe)
                {
                    IWoWUnit newTarget = GetNearestEnemyAttackingGroupMember();
                    if (newTarget != null)
                    {
                        Target = newTarget;
                        Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name}, switching");
                        cancable.Cancel = true;
                        SwitchedTargetFight(Target);
                    }
                }
            }
            else
            {
                // Check for fleeing units
                IWoWUnit fleeUnit = FleeingUnit(_entityCache.TankUnit);
                if (fleeUnit != null)
                {
                    // If we are not already targeting it, target it
                    if (_entityCache.Me.TargetGuid != fleeUnit.Guid)
                    {
                        Target = FleeingUnit(_entityCache.TankUnit);
                        Logger.Log($"TargetingManager: Target fleeing: {Target.Name} , switching");
                        cancable.Cancel = true;
                        SwitchedTargetFight(Target);
                    }
                }
                else
                {
                    //Assist tank
                    IWoWUnit assistTankUnit = AssistTank(_entityCache.TankUnit);
                    if (assistTankUnit != null)
                    {
                        if (assistTankUnit.Guid != _entityCache.Me.TargetGuid)
                        {
                            Target = assistTankUnit;
                            Logger.Log($"TargetingManager: Target attacking Tank: {Target.Name} , switching");
                            cancable.Cancel = true;
                            SwitchedTargetFight(Target);
                        }
                    }
                    else
                    {
                        //Assist any Groupmember if Tank has no target
                        IWoWUnit assistGroupUnit = AssistGroup(_entityCache.TankUnit);
                        if (assistGroupUnit != null)
                        {
                            if (assistGroupUnit.Guid != _entityCache.Me.TargetGuid)
                            {
                                Target = assistGroupUnit;
                                Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name} , switching");
                                cancable.Cancel = true;
                                SwitchedTargetFight(Target);
                            }
                        }
                        else
                        {
                            // Attack targets attacking me if no-one else has a target
                            IWoWUnit attackingMe = AttackingMe();
                            if (attackingMe != null)
                            {
                                if (attackingMe.Guid != _entityCache.Me.TargetGuid)
                                {
                                    Target = attackingMe;
                                    Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name} , switching");
                                    cancable.Cancel = true;
                                    SwitchedTargetFight(Target);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SwitchedTargetFight(IWoWUnit target)
        {
            ObjectManager.Me.Target = Target.Guid;
            Fight.StartFight(target.Guid, false);
        }

        private IWoWUnit GetNearestEnemyAttackingGroupMember()
        {
            return TargetingHelper.FindClosestUnit(unit =>
                unit.IsAttackingGroup && !unit.IsAttackingMe && !unit.Dead && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                _entityCache.Me.PositionWithoutType, _entityCache.EnemyUnitsList);
        }

        private IWoWUnit FleeingUnit(IWoWUnit Tank)
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.Fleeing
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType, _entityCache.EnemyUnitsList);
            return Unit;
        }

        private IWoWUnit AttackingMe()
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.IsAttackingMe
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, PointInMidOfGroup(), _entityCache.EnemyUnitsList);
            return Unit;
        }
        private IWoWUnit AssistTank(IWoWUnit Tank)
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.TargetGuid == Tank.Guid
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType, _entityCache.EnemyUnitsList);
            return Unit;
        }

        private IWoWUnit AssistGroup(IWoWUnit Tank)
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && unit.TargetGuid != Tank.Guid
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, Tank.PositionWithoutType, _entityCache.EnemyUnitsList);
            return Unit;
        }

        private Vector3 PointInMidOfGroup()
        {
            float xvec = 0;
            float yvec = 0;
            float zvec = 0;

            int counter = 0;
            foreach (IWoWUnit player in _entityCache.ListGroupMember)
            {
                xvec = xvec + player.PositionWithoutType.X;
                yvec = yvec + player.PositionWithoutType.Y;
                zvec = zvec + player.PositionWithoutType.Z;

                counter++;
            }

            return new Vector3(xvec / counter, yvec / counter, zvec / counter);
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