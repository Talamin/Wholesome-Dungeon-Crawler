using System.ComponentModel;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    class TargetingManager : ITargetingManager
    {

        private readonly IEntityCache _entityCache;

        public TargetingManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
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

            if (_entityCache.IAmTank)
            {
                if (_entityCache.Target.TargetGuid == _entityCache.Me.Guid)
                {
                    // NPC is attacked, attack him
                    newTarget = GetNearestEnemyAttackingNPCtoProtect();
                    if (newTarget != null)
                    {
                        if (newTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"{newTarget.Name} needs tanking to protect NPC");
                            cancable.Cancel = true;
                            SwitchedTargetFight(newTarget);
                        }
                        return;
                    }

                    // My target is attacking me, check for untanked units
                    newTarget = TargetingHelper.FindClosestUnit(unit =>
                        unit.TargetGuid != _entityCache.Me.Guid
                        && !unit.Fleeing
                        && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                        _entityCache.Me.PositionWithoutType,
                        _entityCache.EnemiesAttackingGroup);

                    if (newTarget != null)
                    {
                        if (newTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"{newTarget.Name} needs tanking to protect group member");
                            cancable.Cancel = true;
                            SwitchedTargetFight(newTarget);
                        }
                        return;
                    }
                }
            }
            else
            {
                // Assist tank
                if (_entityCache.TankUnit != null)
                {
                    newTarget = TargetingHelper.FindClosestUnit(unit =>
                        unit.TargetGuid == _entityCache.TankUnit.Guid
                        && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                        _entityCache.TankUnit.PositionWithoutType,
                        _entityCache.EnemiesAttackingGroup);

                    if (newTarget != null)
                    {
                        if (newTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"Assisting tank against {newTarget.Name}");
                            cancable.Cancel = true;
                            SwitchedTargetFight(newTarget);
                        }
                        return;
                    }
                }

                // Assist any Groupmember if Tank is not here or has no target
                newTarget = TargetingHelper.FindClosestUnit(unit =>
                    _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                    Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember),
                    _entityCache.EnemiesAttackingGroup);

                if (newTarget != null)
                {
                    if (newTarget.Guid != _entityCache.Me.TargetGuid)
                    {
                        Logger.Log($"Assisting Group member against {newTarget.Name}");
                        cancable.Cancel = true;
                        SwitchedTargetFight(newTarget);
                    }
                    return;
                }
            }
        }

        private void SwitchedTargetFight(IWoWUnit target)
        {
            ObjectManager.Me.Target = target.Guid;
            Fight.StartFight(target.Guid, false);
        }

        private IWoWUnit GetNearestEnemyAttackingNPCtoProtect()
        {
            foreach (IWoWUnit npcToDefend in _entityCache.NpcsToDefend)
            {
                return TargetingHelper.FindClosestUnit(unit =>
                    unit.TargetGuid == npcToDefend.Guid,
                    _entityCache.Me.PositionWithoutType,
                    _entityCache.EnemyUnitsList);
            }
            return null;
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
