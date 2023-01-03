using robotManager.Helpful;
using System.ComponentModel;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    class TargetingManager : ITargetingManager
    {
        private readonly IProfileManager _profileManager;
        private readonly IEntityCache _entityCache;
        private static long MIN_TARGET_SWAP_HP = 1000;
        private static readonly long MIN_TARGET_SWAP_RATIO = 2;

        public TargetingManager(
            IEntityCache entityCache,
            IProfileManager profileManager)
        {
            _profileManager = profileManager;
            _entityCache = entityCache;
            MIN_TARGET_SWAP_HP = _entityCache.Me.Health / 4;
        }
        public void Initialize()
        {
            wManager.Events.FightEvents.OnFightLoop += OnFightHandler;
        }

        public void Dispose()
        {
            wManager.Events.FightEvents.OnFightLoop -= OnFightHandler;
        }

        public void OnFightHandler(WoWUnit currentTarget, CancelEventArgs canceable)
        {
            if (_entityCache.Target.IsDead)
            {
                Interact.ClearTarget();
            }

            if (_entityCache.ListGroupMember.Any(member => member.HasDrinkBuff || member.HasFoodBuff)
                && _entityCache.EnemiesAttackingGroup.Length <= 0)
            {
                Logger.Log($"Cancelling fight because someone needs regeneration");
                canceable.Cancel = true;
                MovementManager.StopMove();
                return;
            }

            Vector3 myPos = _entityCache.Me.PositionWithoutType;

            if (_entityCache.IAmTank)
            {
                // Cancel fight if we need to run back to safe spot
                if (_profileManager.CurrentDungeonProfile?.CurrentStep != null
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep
                    && currentTarget != null)
                {
                    PullToSafeSpotStep pullStep = _profileManager.CurrentDungeonProfile.CurrentStep as PullToSafeSpotStep;
                    if (currentTarget.Target >= 0
                        && _entityCache.EnemiesAttackingGroup.Length > 0
                        && !pullStep.IamInSafeSpot 
                        && !pullStep.PositionInSafeSpotFightRange(currentTarget.Position))
                    {
                        canceable.Cancel = true;
                        return;
                    }
                }

                // Don't chase fleers
                if (_entityCache.Target != null 
                    && _entityCache.Target.Fleeing 
                    && _entityCache.EnemiesAttackingGroup.Count() > 0)
                {
                    canceable.Cancel = true;
                    Interact.ClearTarget();
                    return;
                }

                // NPC is currently being tanked or fleeing, lets check there isn't anyone more important
                if (_entityCache.Target == null || _entityCache.Target.TargetGuid == _entityCache.Me.Guid)
                {
                    // Check for escort NPCs being targeted
                    IWoWUnit unitTargetingEscort = _entityCache.EnemyUnitsList
                        .Where(unit => _entityCache.NpcsToDefend.Exists(npcToDefend => npcToDefend.Guid == unit.TargetGuid))
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                        .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                        .FirstOrDefault();
                    if (unitTargetingEscort != null && unitTargetingEscort.Guid != _entityCache.Target.Guid)
                    {
                        Logger.Log($"{unitTargetingEscort.Name} needs tanking to protect NPC");
                        TargetingHelper.SwitchTargetAndFight(unitTargetingEscort, canceable);
                        return;
                    }

                    // Check for untanked units
                    IWoWUnit untankedUnit = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid != _entityCache.Me.Guid
                            && unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                        .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                        .FirstOrDefault();
                    if (untankedUnit != null && untankedUnit.Guid != _entityCache.Target.Guid)
                    {
                        Logger.Log($"{untankedUnit.Name} needs tanking to protect group member");
                        TargetingHelper.SwitchTargetAndFight(untankedUnit, canceable);
                        return;
                    }

                    // Everything is being tanked, switch tank target to lowest HP mob
                    IWoWUnit weakestEnemy = _entityCache.EnemyUnitsList
                        .Where(unit => unit.IsAttackingGroup
                            && unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                        .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                        .FirstOrDefault();
                    if (weakestEnemy != null
                        && weakestEnemy.Guid != _entityCache.Me.TargetGuid
                        && _entityCache.Target != null
                        && _entityCache.Target.Health / weakestEnemy.Health > MIN_TARGET_SWAP_RATIO
                        && _entityCache.Target.Health - weakestEnemy.Health > MIN_TARGET_SWAP_HP)
                    {
                        Logger.Log($"{weakestEnemy.Name} needs finishing off");
                        TargetingHelper.SwitchTargetAndFight(weakestEnemy, canceable);
                        return;
                    }

                    // No current or priority targets, get closest enemy in combat
                    if (_entityCache.Target == null)
                    {
                        IWoWUnit closestEnemy = _entityCache.EnemiesAttackingGroup
                            .Where(unit => unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                            .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                            .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                            .FirstOrDefault();
                        if (closestEnemy != null && closestEnemy.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"{closestEnemy.Name} still needs killing");
                            TargetingHelper.SwitchTargetAndFight(closestEnemy, canceable);
                            return;

                        }
                    }
                }
            }
            else
            {
                // Melee DPS - Don't chase fleers
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.MDPS
                    && _entityCache.Target != null 
                    && _entityCache.Target.Fleeing
                    && _entityCache.EnemiesAttackingGroup.Count() > 0)
                {
                    canceable.Cancel = true;
                    Interact.ClearTarget();
                    return;
                }

                // DPS - Kill Prio Target First
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS
                    || WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.MDPS)
                {
                    // Detect if there is a prio enemy around
                    if (_entityCache.EnemyUnitsList.Any(unit => Lists.ForceTargetListInt.Contains(unit.Entry))
                        && !Lists.ForceTargetListInt.Contains(_entityCache.Target.Entry))
                    {
                        IWoWUnit prioTarget = _entityCache.EnemyUnitsList
                            .Where(unit => Lists.ForceTargetListInt.Contains(unit.Entry)
                                && unit.PositionWithoutType.DistanceTo(myPos) <= 40)
                            .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                            .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                            .FirstOrDefault();
                        if (prioTarget != null && prioTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"Slaughering Prio-Target mob: {prioTarget.Name}");
                            TargetingHelper.SwitchTargetAndFight(prioTarget, canceable);
                            return;
                        }
                    }
                }

                // Ranged DPS - Kill fleers
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS)
                {
                    IWoWUnit fleer = _entityCache.EnemyUnitsList
                        .Where(unit => unit.Fleeing
                            && unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(e => e.HealthPercent)
                        .FirstOrDefault();
                    if (fleer != null && fleer.Guid != _entityCache.Me.TargetGuid)
                    {
                        Logger.Log($"Murdering fleeing mob: {fleer.Name}");
                        TargetingHelper.SwitchTargetAndFight(fleer, canceable);
                        return;
                    }
                }

                if (_entityCache.Target == null)
                {
                    // Assist tank
                    if (_entityCache.TankUnit != null)
                    {
                        IWoWUnit unitTargetedByTank = _entityCache.EnemiesAttackingGroup
                            .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid)
                            .OrderBy(unit => unit.HealthPercent)
                            .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                            .FirstOrDefault();
                        if (unitTargetedByTank != null && unitTargetedByTank.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"Assisting tank against {unitTargetedByTank.Name}");
                            TargetingHelper.SwitchTargetAndFight(unitTargetedByTank, canceable);
                            return;
                        }
                    }

                    // Assist any Groupmember if Tank is not here
                    IWoWUnit unitAttackingMember = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.HealthPercent)
                        .OrderBy(unit => TargetingHelper.GetTargetPriority(unit))
                        .FirstOrDefault();
                    if (unitAttackingMember != null && unitAttackingMember.Guid != _entityCache.Me.TargetGuid)
                    {
                        Logger.Log($"Assisting Group member with {unitAttackingMember.Name}");
                        TargetingHelper.SwitchTargetAndFight(unitAttackingMember, canceable);
                        return;
                    }
                }
            }
        }
    }
}
