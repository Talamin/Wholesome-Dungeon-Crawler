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
        private string _status;

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
                if (_entityCache.Target.Fleeing)
                {
                    Logger.Log($"{foundtarget.Name} is fleeing, switching");
                    _status = foundtarget.Name + " currently fleeing";
                    Interact.ClearTarget();
                }

                IWoWUnit attackerGroupMember = TargetingHelper.FindClosestUnit(unit =>
                    _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
                    && unit.TargetGuid != _entityCache.Me.Guid
                    && !unit.Fleeing,
                    Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember),
                    _entityCache.EnemiesAttackingGroup);
                if (attackerGroupMember != null)
                {
                    foundtarget = attackerGroupMember;
                    _status = "Tanking :"+foundtarget.Name;
                    Logger.Log($"Attacking: {foundtarget.Name} is attacking Groupmember, switching");
                    return true;
                }

                // defend against enemy attacking me
                IWoWUnit attackerMe = TargetingHelper.FindClosestUnit(unit =>
                    unit.TargetGuid == _entityCache.Me.Guid
                    && !unit.Fleeing
                    && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
                    && unit.TargetGuid == _entityCache.Me.Guid,
                    _entityCache.Me.PositionWithoutType,
                    _entityCache.EnemiesAttackingGroup);
                if (attackerMe != null)
                {
                    foundtarget = attackerMe;
                    _status = "Defending :" + foundtarget.Name;
                    Logger.Log($"Attacking: {foundtarget.Name} is attacking Me, switching");
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            Logging.Status = _status;
            MovementManager.StopMove();
            Fight.StopFight();
            Fight.StartFight(foundtarget.Guid, false);
        }
    }
}
