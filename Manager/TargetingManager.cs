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
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Manager
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

            //Tank Section Start
            if(_cache.IAmTank)
            {
                IWoWUnit attackerGroupMember = AttackingGroupMember();
                if (attackerGroupMember != null 
                    && attackerGroupMember.TargetGuid != _entityCache.Me.Guid
                    && attackerGroupMember.TargetGuid != _entityCache.Target.Guid)
                {
                    Target = attackerGroupMember;
                    Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name} , switching");
                    cancable.Cancel = true;
                    SwitchedTargetFight(Target);
                }
            }

            //Tank Section End
            //Slave Section Start
            if (!_cache.IAmTank)
            {
                IWoWUnit fleeUnit = FleeingUnit(_entityCache.TankUnit);
                if (fleeUnit != null && _entityCache.Me.TargetGuid != fleeUnit.Guid)
                {
                    Target = FleeingUnit(_entityCache.TankUnit);
                    Logger.Log($"TargetingManager: Target fleeing: {Target.Name} , switching");
                    cancable.Cancel = true;
                    SwitchedTargetFight(Target);
                }
                //Check to AssistTank
                IWoWUnit assistTankUnit = AssistTank(_entityCache.TankUnit);
                if (assistTankUnit != null && fleeUnit == null
                    && assistTankUnit.Guid != _entityCache.Me.TargetGuid)
                {
                    Target = assistTankUnit;
                    Logger.Log($"TargetingManager: Target attacking Tank: {Target.Name} , switching");
                    cancable.Cancel = true;
                    SwitchedTargetFight(Target);
                }

                //check to Assist any Groupmember if Tank don´t get the aggro
                IWoWUnit assistGroupUnit = AssistGroup(_entityCache.TankUnit);
                if (assistGroupUnit != null && assistTankUnit == null
                    && assistGroupUnit.Guid != _entityCache.Me.TargetGuid)
                {
                    Target = assistGroupUnit;
                    Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name} , switching");
                    cancable.Cancel = true;
                    SwitchedTargetFight(Target);
                }

                IWoWUnit attackingMe = AttackingMe();
                if (attackingMe != null && assistGroupUnit == null
                    && attackingMe.Guid != _entityCache.Me.TargetGuid)
                {
                    Target = attackingMe;
                    Logger.Log($"TargetingManager: Target attacking Groupmember: {Target.Name} , switching");
                    cancable.Cancel = true;
                    SwitchedTargetFight(Target);

                }

            }
            //Slave Section End
        }

        private void SwitchedTargetFight(IWoWUnit target)
        {
            //MovementManager.StopMove();
            //Fight.StopFight();
            //Logger.Log("Start Fight with: " + target.Guid + " Targeting Manager");
            ObjectManager.Me.Target = Target.Guid;
            Fight.StartFight(target.Guid, false);
        }

        private IWoWUnit AttackingGroupMember()
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.IsAttackingGroup
            && !unit.IsAttackingMe
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, PointInMidOfGroup(), _entityCache.EnemyUnitsList);
            return Unit;
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
