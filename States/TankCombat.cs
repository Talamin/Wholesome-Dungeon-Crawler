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
    class TankCombat: State
    {
        public override string DisplayName => "TankCombat";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public TankCombat(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        private IWoWUnit foundtarget;

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || !_cache.IsInInstance
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                if(_entityCache.Target.Dead)
                {
                    Interact.ClearTarget();
                }

                if(!Fight.InFight)
                {
                    IWoWUnit attackerGroupMember = AttackingGroupMember();
                    if (attackerGroupMember != null && attackerGroupMember.TargetGuid != _entityCache.Me.Guid)
                    {
                        foundtarget = attackerGroupMember;
                        Logger.Log($"Attacking: {foundtarget.Name} is attacking Groupmember, switching");
                        return true;
                    }

                    IWoWUnit attackerMe = AttackingMe();
                    if (attackerMe != null && attackerMe.TargetGuid == _entityCache.Me.Guid)
                    {
                        foundtarget = attackerMe;
                        Logger.Log($"Attacking: {foundtarget.Name} is attacking Me, switching");
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
            Fight.StartFight(foundtarget.Guid, false);
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

        private IWoWUnit AttackingMe()
        {
            IWoWUnit Unit = TargetingHelper.FindClosestUnit(unit =>
            unit.IsAttackingMe
            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
            && !unit.Dead, _entityCache.Me.PositionWithoutType, _entityCache.EnemyUnitsList);
            return Unit;
        }

        private Vector3 PointInMidOfGroup()
        {
            float xvec = 0, yvec = 0, zvec = 0;

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
    }
}
