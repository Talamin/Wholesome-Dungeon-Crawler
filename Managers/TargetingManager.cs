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
        private IWoWUnit Target;

        public TargetingManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

        public void OnFightHandler(WoWUnit target, CancelEventArgs cancable)
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

            Target = null;

            if (_entityCache.IAmTank && _entityCache.Target.TargetGuid == _entityCache.Me.Guid)
            {
                // NPC is attacked, attack him
                IWoWUnit newNPCDefendTarget = GetNearestEnemyAttackingNPCtoProtect();
                if (newNPCDefendTarget != null)
                {
                    if (_entityCache.Target.Guid != newNPCDefendTarget.Guid)
                    {
                        Target = newNPCDefendTarget;
                        Logger.Log($"{Target.Name} needs tanking to protect NPC");
                        cancable.Cancel = true;
                        SwitchedTargetFight(Target);
                    }
                    return;
                }

                // My target is attacking me, check for untanked units
                IWoWUnit newTarget = TargetingHelper.FindClosestUnit(unit =>
                    unit.Guid != _entityCache.Target.Guid
                    && !unit.Fleeing
                    && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                    _entityCache.Me.PositionWithoutType,
                    _entityCache.EnemiesAttackingGroup);

                if (newTarget != null)
                {
                    if (newTarget.TargetGuid != _entityCache.Me.Guid)
                    {
                        Target = newTarget;
                        Logger.Log($"{Target.Name} needs tanking to protect group member");
                        cancable.Cancel = true;
                        SwitchedTargetFight(Target);
                    }
                    return;
                }
            }
            else
            {
                // Assist tank
                if (_entityCache.TankUnit != null)
                {
                    IWoWUnit assistTankUnit = TargetingHelper.FindClosestUnit(unit =>
                        unit.TargetGuid == _entityCache.TankUnit.Guid
                        && _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                        _entityCache.TankUnit.PositionWithoutType,
                        _entityCache.EnemiesAttackingGroup);

                    if (assistTankUnit != null)
                    {
                        if (assistTankUnit.TargetGuid != _entityCache.Me.Guid)
                        {
                            Target = assistTankUnit;
                            Logger.Log($"Assisting tank against {Target.Name}");
                            cancable.Cancel = true;
                            SwitchedTargetFight(Target);
                        }
                        return;
                    }
                }

                // Assist any Groupmember if Tank is not here or has no target
                IWoWUnit assistGroupUnit = TargetingHelper.FindClosestUnit(unit =>
                    _entityCache.Me.PositionWithoutType.DistanceTo(unit.PositionWithoutType) <= 60,
                    Toolbox.PointInMidOfGroup(_entityCache.ListGroupMember),
                    _entityCache.EnemiesAttackingGroup);

                if (assistGroupUnit != null)
                {
                    if (assistGroupUnit.Guid != _entityCache.Me.TargetGuid)
                    {
                        Target = assistGroupUnit;
                        Logger.Log($"Assisting Group member against {Target.Name}");
                        cancable.Cancel = true;
                        SwitchedTargetFight(Target);
                    }
                    return;
                }
            }
        }

        private void SwitchedTargetFight(IWoWUnit target)
        {
            ObjectManager.Me.Target = Target.Guid;
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
