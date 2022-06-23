using System.ComponentModel;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    class TargetingManager : ITargetingManager
    {

        private readonly IEntityCache _entityCache;
        private static long MIN_TARGET_SWAP_HP = 1000;
        private static readonly long MIN_TARGET_SWAP_RATIO = 2;
        private bool FleeMessage = false;

        public TargetingManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
            MIN_TARGET_SWAP_HP = _entityCache.Me.Health / 4;
        }

        public void OnFightHandler(WoWUnit currentTarget, CancelEventArgs cancable)
        {
            if (_entityCache.Target.Dead)
            {
                Interact.ClearTarget();
            }

            if (_entityCache.ListGroupMember.Any(member => member.HasDrinkBuff || member.HasFoodBuff)
                && _entityCache.EnemiesAttackingGroup.Length <= 0)
            {
                Logger.Log($"Cancelling fight because someone needs regeneration");
                cancable.Cancel = true;
                MovementManager.StopMove();
                return;
            }

            IWoWUnit newTarget = null;
            IWoWUnit possibleTarget = null;

            if (_entityCache.IAmTank) 
            {
                // Set target for Tank
                // NPC is currently being tanked or fleeing, lets check there isn't anyone more important
                if (_entityCache.Target == null || _entityCache.Target.TargetGuid == _entityCache.Me.Guid || _entityCache.Target.Fleeing)
                {
                    if (_entityCache.Target.Fleeing && FleeMessage == false)
                    {
                        FleeMessage = true;
                        Logger.Log($"{_entityCache.Target.Name} is fleeing, I hope there is someone else to attack!");
                    }

                    // Check for escort NPCs being targeted
                    possibleTarget = GetNearestEnemyAttackingNPCtoProtect();
                    if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                    {
                        newTarget = possibleTarget;
                        Logger.Log($"{newTarget.Name} needs tanking to protect NPC");
                    }

                    if (newTarget == null)
                    {
                        // Check for untanked units
                        possibleTarget = TargetingHelper.FindClosestUnit(unit =>
                            unit.TargetGuid != _entityCache.Me.Guid
                            && !unit.Fleeing
                            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                            _entityCache.Me.PositionWithoutType,
                            _entityCache.EnemiesAttackingGroup);
                        if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            newTarget = possibleTarget;
                            Logger.Log($"{newTarget.Name} needs tanking to protect group member");
                        }
                    }

                    // Everything is being tanked, switch tank target to lowest HP mob
                    if (newTarget == null)
                    {
                        possibleTarget = GetWeakestEnemyUnit();
                        // Don't switch unless the the current mob has both x times the HP of the weakest one and the difference is more than y
                        if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid && _entityCache.Target != null
                            && _entityCache.Target.Health / possibleTarget.Health > MIN_TARGET_SWAP_RATIO
                            && _entityCache.Target.Health - possibleTarget.Health > MIN_TARGET_SWAP_HP)
                        {
                            newTarget = possibleTarget;
                            Logger.Log($"{newTarget.Name} needs finishing off");
                        }
                    }
                    if (newTarget == null && (_entityCache.Target == null || _entityCache.Target.Fleeing))
                    {
                        // No current or priority targets, get closest enemy in combat
                        possibleTarget = TargetingHelper.FindClosestUnit(unit =>
                            !unit.Fleeing
                            && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                            _entityCache.Me.PositionWithoutType,
                            _entityCache.EnemiesAttackingGroup);
                        if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            newTarget = possibleTarget;
                            Logger.Log($"{newTarget.Name} still needs killing");
                        }
                    }
                }
            }
            else
            {   // Set target for DPS
                // Kill fleers
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS)
                {
                    possibleTarget = GetNearestFleeingMob();
                    if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                    {
                        newTarget = possibleTarget;
                        Logger.Log($"Murdering fleeing mob: {newTarget.Name}");
                    }
                }
                // Assist tank
                if (_entityCache.Target == null && newTarget == null && _entityCache.TankUnit != null)
                {
                    possibleTarget = TargetingHelper.FindClosestUnit(unit =>
                        unit.TargetGuid == _entityCache.TankUnit.Guid
                        && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                        _entityCache.TankUnit.PositionWithoutType,
                        _entityCache.EnemiesAttackingGroup);

                    if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                    {                        
                            newTarget = possibleTarget;
                            Logger.Log($"Assisting tank with {newTarget.Name}");                                                  
                    }
                }
                if (_entityCache.Target == null && newTarget == null)
                {
                    // Assist any Groupmember if Tank is not here or has no target
                    possibleTarget = TargetingHelper.FindClosestUnit(unit =>
                        _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                        Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember),
                        _entityCache.EnemiesAttackingGroup);

                    if (possibleTarget != null && possibleTarget.Guid != _entityCache.Me.TargetGuid)
                    {
                        newTarget = possibleTarget;
                        Logger.Log($"Assisting Group member with {newTarget.Name}");
                    }
                }                
            }
            // Actually swap to target
            if (newTarget != null && newTarget.Guid != _entityCache.Me.TargetGuid)
            {
                FleeMessage = false;
                cancable.Cancel = true;
                ObjectManager.Me.Target = newTarget.Guid;
                Fight.StartFight(newTarget.Guid, false);
            }
        }

        private IWoWUnit GetNearestEnemyAttackingNPCtoProtect()
        {
            IWoWUnit result = null;
            foreach (IWoWUnit npcToDefend in _entityCache.NpcsToDefend)
            {
                result = TargetingHelper.FindClosestUnit(unit =>
                    unit.TargetGuid == npcToDefend.Guid,
                    _entityCache.Me.PositionWithoutType,
                    _entityCache.EnemyUnitsList);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private IWoWUnit GetWeakestEnemyUnit()
        {
            return _entityCache.EnemyUnitsList.Where(e => e.IsAttackingGroup && !e.Dead && !e.Fleeing).OrderBy(e => _entityCache.Me.PositionWithoutType.DistanceTo(e.PositionWithoutType)).FirstOrDefault();
        }

        private IWoWUnit GetNearestFleeingMob()
        {
            return _entityCache.EnemyUnitsList.Where(e => e.Fleeing).OrderBy(e => e.Health).FirstOrDefault();

        }

        public void Initialize()
        {
            wManager.Events.FightEvents.OnFightLoop += OnFightHandler;
        }

        public void Dispose()
        {
            wManager.Events.FightEvents.OnFightLoop -= OnFightHandler;
        }
    }
}
