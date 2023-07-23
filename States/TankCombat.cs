using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class TankCombat : State
    {
        public override string DisplayName => "Tank Combat";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private IWoWUnit _foundtarget;

        public TankCombat(
            ICache iCache, 
            IEntityCache entityCache,
            IProfileManager profileManager)
        {
            _cache = iCache;
            _entityCache = entityCache;
            _profileManager = profileManager;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_cache.IsInInstance
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                // Block state if pulling to safe spot
                if (_profileManager.ProfileIsRunning
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                {
                    return false;
                }

                if (_entityCache.Target.IsDead)
                {
                    Interact.ClearTarget();
                }

                if (_entityCache.Target.Fleeing)
                {
                    Logger.Log($"{_entityCache.Target.Name} is fleeing, switching");
                    Interact.ClearTarget();
                }

                _foundtarget = null;

                IWoWUnit attackingGroupMember = _entityCache.EnemiesAttackingGroup
                    .Where(unit => _entityCache.Me.PositionWT.DistanceTo(unit.PositionWT) <= 60
                        && unit.TargetGuid != _entityCache.Me.Guid
                        && !unit.Fleeing)
                    .OrderBy(unit => unit.PositionWT.DistanceTo(Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember)))
                    .FirstOrDefault();
                if (attackingGroupMember != null)
                {
                    _foundtarget = attackingGroupMember;
                    Logger.Log($"TankCombat: {_foundtarget.Name} is attacking groupmember, defending");
                    return true;
                }

                // defend against enemy attacking me
                IWoWUnit attackerMe = _entityCache.EnemiesAttackingGroup
                    .Where(unit => _entityCache.Me.PositionWT.DistanceTo(unit.PositionWT) <= 60
                        && unit.TargetGuid == _entityCache.Me.Guid
                        && !unit.Fleeing)
                    .OrderBy(unit => unit.PositionWT.DistanceTo(_entityCache.Me.PositionWT))
                    .FirstOrDefault();
                if (attackerMe != null)
                {
                    _foundtarget = attackerMe;
                    Logger.Log($"Attacking: {_foundtarget.Name} is attacking Me, switching");
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();
            Fight.StopFight();
            Fight.StartFight(_foundtarget.Guid, false);
        }
    }
}
