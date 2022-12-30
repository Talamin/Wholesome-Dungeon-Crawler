﻿using robotManager.FiniteStateMachine;
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
        public override string DisplayName => "TankCombat";

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
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || !_cache.IsInInstance
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                // Block state if pulling to safe spot
                if (_profileManager.CurrentDungeonProfile?.CurrentStep != null
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                {
                    PullToSafeSpotStep step = _profileManager.CurrentDungeonProfile.CurrentStep as PullToSafeSpotStep;
                    if (!step.IamInSafeSpot)
                    {
                        return false;
                    }
                }

                if (_entityCache.Target.Dead)
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
                    .Where(unit => _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
                        && unit.TargetGuid != _entityCache.Me.Guid
                        && !unit.Fleeing)
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember)))
                    .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                    .FirstOrDefault();
                if (attackingGroupMember != null)
                {
                    _foundtarget = attackingGroupMember;
                    Logger.Log($"TankCombat: {_foundtarget.Name} is attacking groupmember, defending");
                    return true;
                }

                // defend against enemy attacking me
                IWoWUnit attackerMe = _entityCache.EnemiesAttackingGroup
                    .Where(unit => _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60
                        && unit.TargetGuid == _entityCache.Me.Guid
                        && !unit.Fleeing)
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType))
                    .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
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
