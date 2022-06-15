using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class TankCombat : State
    {
        public override string DisplayName => "TankCombat";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public TankCombat(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
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

                if (_entityCache.Target.Dead)
                {
                    Interact.ClearTarget();
                }

                IWoWUnit attackerGroupMember = TargetingHelper.FindClosestUnit(unit =>
                    _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                    Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember),
                    _entityCache.EnemiesAttackingGroup);
                if (attackerGroupMember != null && attackerGroupMember.TargetGuid != _entityCache.Me.Guid)
                {
                    foundtarget = attackerGroupMember;
                    Logger.Log($"Attacking: {foundtarget.Name} is attacking Groupmember, switching");
                    return true;
                }

                // defend against enemy attacking me
                IWoWUnit attackerMe = TargetingHelper.FindClosestUnit(unit =>
                    unit.TargetGuid == _entityCache.Me.Guid
                    && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                    _entityCache.Me.PositionWithoutType,
                    _entityCache.EnemiesAttackingGroup);
                if (attackerMe != null && attackerMe.TargetGuid == _entityCache.Me.Guid)
                {
                    foundtarget = attackerMe;
                    Logger.Log($"Attacking: {foundtarget.Name} is attacking Me, switching");
                    return true;
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
    }
}
