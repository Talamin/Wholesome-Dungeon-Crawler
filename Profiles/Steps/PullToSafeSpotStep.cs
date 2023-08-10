using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class PullToSafeSpotStep : Step
    {
        private readonly PullToSafeSpotModel _pullToSafeSpotModel;
        private readonly IEntityCache _entityCache;
        private readonly Vector3 _safeSpotCenter;
        private readonly int _safeSpotRadius;
        private readonly Vector3 _zoneToClearPosition;
        private readonly int _zoneToClearRadius;
        private readonly int _zoneToClearZLimit;
        private readonly Dictionary<ulong, List<Vector3>> _pathsToEnemiesToPull = new Dictionary<ulong, List<Vector3>>(); // unit -> path to unit from safespot
        private readonly Dictionary<ulong, PulledEnemy> _pulledEnemiesDic = new Dictionary<ulong, PulledEnemy>(); // used to detect standing still enemies outside safe zone
        private List<ICachedWoWUnit> _enemiesStandingStill = new List<ICachedWoWUnit>();
        private List<WoWUnit> _enemiesToPull = new List<WoWUnit>();

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }

        public bool PositionInSafeSpotFightRange(Vector3 position) =>
            position.DistanceTo(_safeSpotCenter) <= _safeSpotRadius
            && !TraceLine.TraceLineGo(_safeSpotCenter, position);
        public bool EnemyIsStandingStill(ulong guid) => _enemiesStandingStill.Exists(e => e.Guid == guid);
        public bool AnEnemyIsStandingStill => _enemiesStandingStill.Count > 0;
        public Vector3 SafeSpotCenter => _safeSpotCenter;

        public PullToSafeSpotStep(
            PullToSafeSpotModel pullToSafeSpotModel,
            IEntityCache entityCache) : base(pullToSafeSpotModel.CompleteCondition)
        {
            Name = pullToSafeSpotModel.Name;
            StepFaction = pullToSafeSpotModel.StepFaction;
            StepRole = pullToSafeSpotModel.StepRole;
            _pullToSafeSpotModel = pullToSafeSpotModel;
            _entityCache = entityCache;
            _safeSpotRadius = pullToSafeSpotModel.SafeSpotRadius;
            _safeSpotCenter = pullToSafeSpotModel.SafeSpotPosition;
            _zoneToClearPosition = pullToSafeSpotModel.ZoneToClearPosition;
            _zoneToClearRadius = pullToSafeSpotModel.ZoneToClearRadius;
            _zoneToClearZLimit = pullToSafeSpotModel.ZoneToClearZLimit;
            PreEvaluationPass = EvaluateFactionCompletion();

            if (_safeSpotCenter == null)
            {
                Logger.LogError($"ERROR: The safe spot of your current step {Name} is null!");
            }
            if (_zoneToClearPosition == null)
            {
                Logger.LogError($"ERROR: The zone to clear position of your current step {Name} is null!");
            }
        }

        public override void Initialize()
        {
            if (!Radar3D.IsLaunched) Radar3D.Pulse();
            Radar3D.OnDrawEvent += OnDrawEvent;
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            FightEvents.OnFightLoop += OnFightHandler;
            FightEvents.OnFightStart += OnFightHandler;
        }

        public override void Dispose()
        {
            Radar3D.OnDrawEvent -= OnDrawEvent;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            FightEvents.OnFightLoop -= OnFightHandler;
            FightEvents.OnFightStart -= OnFightHandler;
        }

        private void OnObjectManagerPulse()
        {
            // Detect Standing still enemies
            if (_entityCache.EnemiesAttackingGroup.Length > 0)
            {
                foreach (ICachedWoWUnit unit in _entityCache.EnemiesAttackingGroup)
                {
                    if (_pulledEnemiesDic.TryGetValue(unit.Guid, out PulledEnemy enemyPulled))
                    {
                        enemyPulled.RecordPosition(unit.PositionWT, _safeSpotCenter, _safeSpotRadius);
                    }
                    else
                    {
                        _pulledEnemiesDic.Add(unit.Guid, new PulledEnemy(unit));
                    }
                }
            }

            // Record enemies standing still in a list
            _enemiesStandingStill = _entityCache.EnemyUnitsList
                .Where(e => _pulledEnemiesDic.ContainsKey(e.Guid)
                    && _pulledEnemiesDic[e.Guid].IsStandingStill
                    && e.InCombatFlagOnly
                    && !e.IsDead
                    && e.WowUnit.IsAttackable
                    && !e.WowUnit.NotSelectable)
                .OrderBy(e => _safeSpotCenter.DistanceTo(e.PositionWT))
                .ToList();
        }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

            if (_entityCache.Me.IsDead)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
                return;
            }

            // If an enemy is in the safe spot
            ICachedWoWUnit enemyClosestFromSafeSpot = _entityCache.EnemyUnitsList
                .OrderBy(e => e.PositionWT.DistanceTo(_safeSpotCenter))
                .FirstOrDefault(e => PositionInSafeSpotFightRange(e.PositionWT));
            if (enemyClosestFromSafeSpot != null)
            {
                Logger.Log($"{enemyClosestFromSafeSpot.Name} is in the safe zone. Attacking.");
                Fight.StartFight(enemyClosestFromSafeSpot.Guid);
                return;
            }

            // Attack standing still enemy
            if (_enemiesStandingStill.Count > 0)
            {
                ICachedWoWUnit standingStillToAttack = _enemiesStandingStill.First();
                Logger.Log($"{standingStillToAttack.Name} is standing still. Attacking.");
                Fight.StartFight(standingStillToAttack.Guid);
                return;
            }

            // Record enemies in the pull zone
            if (_entityCache.EnemiesAttackingGroup.Length <= 0)
            {
                _enemiesToPull = ObjectManager.GetObjectWoWUnit()
                .Where(unit => unit.Position.Z <= _zoneToClearPosition.Z + _zoneToClearZLimit
                    && unit.Position.Z >= _zoneToClearPosition.Z - _zoneToClearZLimit
                    && !Lists.MobsToIgnoreDuringSteps.Contains(unit.Entry)
                    && unit.IsAttackable
                    && unit.Reaction <= wManager.Wow.Enums.Reaction.Unfriendly)
                .Where(unit => unit.Position.DistanceTo(_zoneToClearPosition) <= _zoneToClearRadius)
                .ToList();
                Logger.LogOnce($"{_enemiesToPull.Count} enemies left to pull");
            }

            // TANK LOGIC
            if (_entityCache.IAmTank)
            {
                // Go to safe spot
                if (!IsUnitAtSafeSPot(_entityCache.Me) && !MovementManager.InMovement)
                {
                    Logger.Log($"Going to safe spot");
                    List<Vector3> pathToSafeSpot = PathFinder.FindPath(_entityCache.Me.PositionWT, _safeSpotCenter);
                    MovementManager.Go(pathToSafeSpot);
                    return;
                }

                // Search for enemies to pull when in safe spot
                if (_entityCache.EnemiesAttackingGroup.Length <= 0)
                {
                    if (_entityCache.ListGroupMember.Any(m => m.InCombatFlagOnly))
                    {
                        Logger.LogOnce($"Waiting for combat flags to wear off");
                        return;
                    }

                    // Record path distances
                    foreach (WoWUnit enemy in _enemiesToPull)
                    {
                        if (_pathsToEnemiesToPull.TryGetValue(enemy.Guid, out List<Vector3> savedPath)) // readjust paths for patrols
                        {
                            if (savedPath == null || savedPath.Last().DistanceTo(enemy.Position) > 5f)
                            {
                                _pathsToEnemiesToPull.Remove(enemy.Guid);
                                List<Vector3> pathToEnemy = PathFinder.FindPath(_safeSpotCenter, enemy.Position);
                                _pathsToEnemiesToPull.Add(enemy.Guid, pathToEnemy);
                                Logger.Log($"Adjusted {enemy.Name} path (enemy is patroling)");
                            }
                        }
                        else // Add path to dic
                        {
                            List<Vector3> pathToEnemy = PathFinder.FindPath(_safeSpotCenter, enemy.Position);
                            _pathsToEnemiesToPull.Add(enemy.Guid, pathToEnemy);
                            Logger.Log($"Added {enemy.Name} path with path distance {WTPathFinder.CalculatePathTotalDistance(pathToEnemy)} yards");
                        }
                    }

                    // Search for closest enemy to pull
                    KeyValuePair<ulong, List<Vector3>> closestEntry = _pathsToEnemiesToPull
                        .Where(kvp => kvp.Value != null)
                        .OrderBy(kvp => WTPathFinder.CalculatePathTotalDistance(kvp.Value))
                        .FirstOrDefault();
                    if (closestEntry.Key > 0)
                    {
                        WoWUnit unitToPull = _enemiesToPull
                            .Where(unit => unit.Guid == closestEntry.Key)
                            .FirstOrDefault();

                        // Unit is absent or dead, remove path from dictionary
                        if (unitToPull == null || unitToPull.IsDead)
                        {
                            _pathsToEnemiesToPull.Remove(closestEntry.Key);
                            return;
                        }

                        // Start pulling
                        bool teammatesAtSafeSpot = _entityCache.ListGroupMember
                            .All(m => m.Guid == _entityCache.Me.Guid || IsUnitAtSafeSPot(m));

                        if (teammatesAtSafeSpot)
                        {
                            // Attack enemy in sight
                            foreach (WoWUnit unit in _enemiesToPull)
                            {
                                if (_entityCache.Me.PositionWT.DistanceTo(unit.Position) < 30
                                    && !TraceLine.TraceLineGo(_entityCache.Me.PositionWT, unit.Position))
                                {
                                    MovementManager.StopMove();
                                    Logger.Log($"Taking aggro on {unitToPull.Name}");
                                    Fight.StartFight(unit.Guid);
                                    return;
                                }
                            }

                            // Move toward enemy to pull
                            if (!MovementManager.InMovement)
                            {
                                Logger.Log($"Pulling {unitToPull.Name} to safe spot");
                                MovementManager.Go(WTPathFinder.PathFromClosestPoint(closestEntry.Value));
                                return;
                            }
                        }
                        else
                        {
                            //Logger.LogOnce($"Waiting for the team to regroup at safe spot");
                        }
                        return;
                    }
                }
                else
                {
                    // Aggro during pull, return to safespot
                    if (MovementManager.CurrentPath.Last() != _safeSpotCenter)
                    {
                        MovementManager.StopMove();
                        return;
                    }
                }
            }
            // FOLLOWER LOGIC
            {
                if (_entityCache.EnemiesAttackingGroup.Length <= 0)
                {
                    Interact.ClearTarget();
                    // Go to safe spot
                    if (!IsUnitAtSafeSPot(_entityCache.Me) && !MovementManager.InMovement)
                    {
                        Logger.Log($"Going to safe spot");
                        MovementManager.StopMove();
                        List<Vector3> pathToSafeSpot = PathFinder.FindPath(_entityCache.Me.PositionWT, _safeSpotCenter);
                        MovementManager.Go(WTPathFinder.PathFromClosestPoint(WTPathFinder.PathFromClosestPoint(pathToSafeSpot)));
                        return;
                    }
                }
            }

            // Complete condition
            if (_enemiesToPull.Count <= 0
                && _entityCache.EnemiesAttackingGroup.Length <= 0
                && _entityCache.ListGroupMember.All(m => !m.InCombatFlagOnly && IsUnitAtSafeSPot(m))
                && EvaluateCompleteCondition())
            {
                _pulledEnemiesDic.Clear();
                _pathsToEnemiesToPull.Clear();
                MarkAsCompleted();
            }
        }

        private bool IsUnitAtSafeSPot(ICachedWoWUnit unit) => unit.PositionWT.DistanceTo(_safeSpotCenter) < 3.5f;

        private void OnFightHandler(WoWUnit currentTarget, CancelEventArgs canceable)
        {
            try
            {
                if (_entityCache.EnemiesAttackingGroup.Length > 0
                    && !AnEnemyIsStandingStill
                    && _entityCache.EnemiesAttackingGroup.All(e => !PositionInSafeSpotFightRange(e.PositionWT))
                    && _entityCache.ListGroupMember.All(g => g.TargetGuid <= 0 || g.WowUnit.TargetObject.Reaction > Reaction.Hostile))
                {
                    Logger.Log($"Cancelling fight (Pull Step)");
                    canceable.Cancel = true;
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        private void OnDrawEvent()
        {
            try
            {
                Radar3D.DrawCircle(_safeSpotCenter, _safeSpotRadius, Color.Green, false, 50);
                Radar3D.DrawCircle(_safeSpotCenter, 1, Color.Green, true, 50);
                Radar3D.DrawCircle(_zoneToClearPosition, _zoneToClearRadius, Color.Red, false, 50);
                Radar3D.DrawCircle(_zoneToClearPosition, 1, Color.Red, true, 50);

                foreach (ICachedWoWUnit unit in _entityCache.EnemiesAttackingGroup)
                {
                    if (PositionInSafeSpotFightRange(unit.PositionWT))
                    {
                        Radar3D.DrawLine(_entityCache.Me.PositionWT, unit.PositionWT, Color.Purple);
                        Radar3D.DrawCircle(unit.PositionWT, 0.5f, Color.Purple, true, 100);
                    }
                    else
                    {
                        Radar3D.DrawLine(_entityCache.Me.PositionWT, unit.PositionWT, Color.White);
                        Radar3D.DrawCircle(unit.PositionWT, 0.5f, Color.White, true, 100);
                    }
                }

                foreach (ICachedWoWUnit unit in _enemiesStandingStill)
                {
                    Radar3D.DrawLine(_entityCache.Me.PositionWT, unit.PositionWT, Color.Yellow);
                    Radar3D.DrawCircle(unit.PositionWT, 0.7f, Color.Yellow, false, 100);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        private class PulledEnemy
        {
            private Vector3 _lastRecordedPosition;
            private Timer _recordTimer;
            private float _standingStillSeconds;
            private float _maxSecsStandingStill = 5;

            public ulong Guid { get; private set; }
            public bool IsStandingStill => _standingStillSeconds > _maxSecsStandingStill;

            public PulledEnemy(ICachedWoWUnit unit)
            {
                Guid = unit.Guid;
            }

            public void RecordPosition(Vector3 position, Vector3 safeSpotCenter, int safeSpotRadius)
            {
                if (!IsStandingStill)
                {
                    if (_recordTimer == null || _recordTimer.IsReady)
                    {
                        // The closer the enemmy is, the faster it is considered as standing still
                        float increment;
                        float distanceToSafeSpot = position.DistanceTo(safeSpotCenter);
                        if (distanceToSafeSpot - safeSpotRadius > 25) increment = 0.5f;
                        else if (distanceToSafeSpot - safeSpotRadius > 15) increment = 1;
                        else if (distanceToSafeSpot - safeSpotRadius > 5) increment = 2;
                        else increment = 3;

                        if (_lastRecordedPosition == position)
                        {
                            _standingStillSeconds += increment;
                        }
                        else
                        {
                            _standingStillSeconds = 0;
                            _lastRecordedPosition = position;
                        }
                        _recordTimer = new Timer(1000);
                    }
                }
            }
        }
    }
}
