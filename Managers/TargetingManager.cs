using robotManager.Helpful;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeDungeonCrawler.Helpers.TargetingHelper;

namespace WholesomeDungeonCrawler.Managers
{
    class TargetingManager : ITargetingManager
    {
        private readonly IProfileManager _profileManager;
        private readonly IEntityCache _entityCache;
        private readonly long MIN_TARGET_SWAP_HP = 1000;
        private readonly long MIN_TARGET_SWAP_RATIO = 2;
        private readonly List<IWoWUnit> _lowPrioUnits = new List<IWoWUnit>();
        private readonly List<IWoWUnit> _highPrioUnits = new List<IWoWUnit>();

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
            if (!Radar3D.IsLaunched) Radar3D.Pulse();
            Radar3D.OnDrawEvent += DrawEventTargetingManager;
        }

        public void Dispose()
        {
            wManager.Events.FightEvents.OnFightLoop -= OnFightHandler;
            Radar3D.OnDrawEvent -= DrawEventTargetingManager;
            Radar3D.Stop();
        }

        public void OnFightHandler(WoWUnit currentTarget, CancelEventArgs canceable)
        {
            if (currentTarget == null)
            {
                return;
            }

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
                if (_profileManager.ProfileIsRunning
                    && _profileManager.CurrentDungeonProfile.CurrentStep is PullToSafeSpotStep)
                {
                    PullToSafeSpotStep pullStep = _profileManager.CurrentDungeonProfile.CurrentStep as PullToSafeSpotStep;
                    if (currentTarget.Target >= 0
                        && _entityCache.EnemiesAttackingGroup.Length > 0
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
                        .FirstOrDefault();
                    if (unitTargetingEscort != null && unitTargetingEscort.Guid != _entityCache.Target.Guid)
                    {
                        Logger.Log($"{unitTargetingEscort.Name} needs tanking to protect NPC");
                        SwitchTargetAndFight(unitTargetingEscort, canceable);
                        return;
                    }

                    // Check for untanked units
                    IWoWUnit untankedUnit = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.TargetGuid != _entityCache.Me.Guid
                            && unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                        .FirstOrDefault();
                    if (untankedUnit != null && untankedUnit.Guid != _entityCache.Target.Guid)
                    {
                        Logger.Log($"{untankedUnit.Name} needs tanking to protect group member");
                        SwitchTargetAndFight(untankedUnit, canceable);
                        return;
                    }

                    // Everything is being tanked, switch tank target to lowest HP mob
                    IWoWUnit weakestEnemy = _entityCache.EnemiesAttackingGroup
                        .Where(unit => unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                        .FirstOrDefault();
                    if (weakestEnemy != null
                        && weakestEnemy.Guid != _entityCache.Me.TargetGuid
                        && _entityCache.Target != null
                        && _entityCache.Target.Health / weakestEnemy.Health > MIN_TARGET_SWAP_RATIO
                        && _entityCache.Target.Health - weakestEnemy.Health > MIN_TARGET_SWAP_HP)
                    {
                        Logger.Log($"{weakestEnemy.Name} needs finishing off");
                        SwitchTargetAndFight(weakestEnemy, canceable);
                        return;
                    }

                    // No current or priority targets, get closest enemy in combat
                    if (_entityCache.Target == null)
                    {
                        IWoWUnit closestEnemy = _entityCache.EnemiesAttackingGroup
                            .Where(unit => unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                            .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                            .FirstOrDefault();
                        if (closestEnemy != null && closestEnemy.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"{closestEnemy.Name} still needs killing");
                            SwitchTargetAndFight(closestEnemy, canceable);
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

                _lowPrioUnits.Clear();
                _highPrioUnits.Clear();
                List<IWoWUnit> filteredEnemies = new List<IWoWUnit>(_entityCache.EnemyUnitsList);
                foreach (IWoWUnit enemy in filteredEnemies)
                {
                    if (Lists.SpecialPrioTargets.TryGetValue(enemy.Entry, out SpecialPrio prio))
                    {
                        if (prio.WhenAttackingGroup && !enemy.IsAttackingGroup) continue;
                        if (prio.WhenInFightWith > 0 && !filteredEnemies.Any(ifEnemy => ifEnemy.IsAttackingGroup && ifEnemy.Entry == prio.WhenInFightWith)) continue;
                        if (prio.TargetPriority == TargetPriority.Low) _lowPrioUnits.Add(enemy);
                        if (prio.TargetPriority == TargetPriority.High) _highPrioUnits.Add(enemy);
                    }
                }

                // If other units are fighting
                if (_entityCache.EnemiesAttackingGroup.Count() > _lowPrioUnits.Count)
                {
                    // We're targeting a low prio, cancel fight
                    if (_lowPrioUnits.Any(lpu => _entityCache.Me.TargetGuid == lpu.Guid))
                    {
                        IWoWUnit newUnit = _entityCache.EnemiesAttackingGroup
                            .Where(enemy => !_lowPrioUnits.Exists(en => en.Guid == enemy.Guid))
                            .OrderBy(enemy => enemy.HealthPercent)
                            .FirstOrDefault();
                        if (newUnit != null)
                        {
                            Logger.Log($"Atacking better target: {newUnit.Name}");
                            SwitchTargetAndFight(newUnit, canceable);
                            return;
                        }
                    }
                    // We remove all the low prios from the filtered list
                    filteredEnemies.RemoveAll(fe => _lowPrioUnits.Exists(lpu => lpu.Entry == fe.Entry));
                }

                // DPS - Handle prios
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS
                    || WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.MDPS)
                {
                    if (_highPrioUnits.Count > 0)
                    {
                        IWoWUnit prioTarget = _highPrioUnits
                            .OrderBy(unit => unit.HealthPercent)
                            .FirstOrDefault();
                        if (prioTarget.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"Atacking high prio target: {prioTarget.Name}");
                            SwitchTargetAndFight(prioTarget, canceable);
                            return;
                        }
                    }
                }

                // Ranged DPS - Kill fleers
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS)
                {
                    IWoWUnit fleer = _entityCache.EnemyUnitsList
                        .Where(unit => unit.Fleeing && unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(e => e.HealthPercent)
                        .FirstOrDefault();
                    if (fleer != null && fleer.Guid != _entityCache.Me.TargetGuid)
                    {
                        Logger.Log($"Atacking fleeing mob: {fleer.Name}");
                        SwitchTargetAndFight(fleer, canceable);
                        return;
                    }
                }

                if (_entityCache.Target == null)
                {
                    // Assist tank
                    if (_entityCache.TankUnit != null)
                    {
                        IWoWUnit unitTargetedByTank = filteredEnemies
                            .Where(unit => unit.TargetGuid == _entityCache.TankUnit.Guid)
                            .OrderBy(unit => unit.HealthPercent)
                            .FirstOrDefault();
                        if (unitTargetedByTank != null && unitTargetedByTank.Guid != _entityCache.Me.TargetGuid)
                        {
                            Logger.Log($"Assisting tank against: {unitTargetedByTank.Name}");
                            SwitchTargetAndFight(unitTargetedByTank, canceable);
                            return;
                        }
                    }

                    // Assist any Groupmember if Tank is not here
                    IWoWUnit unitAttackingMember = filteredEnemies
                        .Where(unit => unit.PositionWithoutType.DistanceTo(myPos) <= 60)
                        .OrderBy(unit => unit.HealthPercent)
                        .FirstOrDefault();
                    if (unitAttackingMember != null && unitAttackingMember.Guid != _entityCache.Me.TargetGuid)
                    {
                        Logger.Log($"Assisting group member against: {unitAttackingMember.Name}");
                        SwitchTargetAndFight(unitAttackingMember, canceable);
                        return;
                    }
                }
            }
        }

        private void DrawEventTargetingManager()
        {
            foreach (IWoWUnit unit in _highPrioUnits)
            {
                Radar3D.DrawCircle(unit.PositionWithoutType, 1f, Color.Yellow, true, 30);
            }
            foreach (IWoWUnit unit in _lowPrioUnits)
            {
                Radar3D.DrawCircle(unit.PositionWithoutType, 1f, Color.Red, true, 30);
            }
        }
    }
}
