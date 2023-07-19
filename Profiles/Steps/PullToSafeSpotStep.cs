using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

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
        private readonly Dictionary<ulong, List<Vector3>> _enemiesToClear = new Dictionary<ulong, List<Vector3>>(); // unit -> path to unit from safespot
        private readonly Dictionary<ulong, PulledEnemy> _pulledEnemiesDic = new Dictionary<ulong, PulledEnemy>(); // used to detect standing still enemies outside safe zone
        private List<IWoWUnit> _enemiesStandingStill = new List<IWoWUnit>();

        public override string Name { get; }
        public bool PositionInSafeSpotFightRange(Vector3 position) =>
            position.DistanceTo(_safeSpotCenter) <= _safeSpotRadius
            && !TraceLine.TraceLineGo(_safeSpotCenter, position);
        public bool AnEnemyIsStandingStill => _enemiesStandingStill.Count > 0;
        public Vector3 SafeSportCenter => _safeSpotCenter;

        public PullToSafeSpotStep(
            PullToSafeSpotModel pullToSafeSpotModel,
            IEntityCache entityCache) : base(pullToSafeSpotModel.CompleteCondition)
        {
            Name = pullToSafeSpotModel.Name;
            _pullToSafeSpotModel = pullToSafeSpotModel;
            _entityCache = entityCache;
            _safeSpotRadius = pullToSafeSpotModel.SafeSpotRadius;
            _safeSpotCenter = pullToSafeSpotModel.SafeSpotPosition;
            _zoneToClearPosition = pullToSafeSpotModel.ZoneToClearPosition;
            _zoneToClearRadius = pullToSafeSpotModel.ZoneToClearRadius;
            _zoneToClearZLimit = pullToSafeSpotModel.ZoneToClearZLimit;

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
            Radar3D.OnDrawEvent += DrawEventPathManager;
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
        }

        public override void Dispose()
        {
            Radar3D.OnDrawEvent -= DrawEventPathManager;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }

        private void OnObjectManagerPulse()
        {
            // Detect Standing still enemies
            if (_entityCache.EnemiesAttackingGroup.Length > 0)
            {
                foreach (IWoWUnit unit in _entityCache.EnemiesAttackingGroup)
                {
                    if (_pulledEnemiesDic.TryGetValue(unit.Guid, out PulledEnemy enemyPulled))
                    {
                        enemyPulled.RecordPosition(unit.PositionWithoutType);
                    }
                    else
                    {
                        _pulledEnemiesDic.Add(unit.Guid, new PulledEnemy(unit));
                    }
                }
            }
            else
            {
                _pulledEnemiesDic.Clear();
            }
        }

        public override void Run()
        {
            if (_entityCache.Me.IsDead)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
                return;
            }

            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            List<WoWUnit> enemiesToPull = ObjectManager.GetWoWUnitHostile()
                .Where(unit => unit.Position.Z <= _zoneToClearPosition.Z + _zoneToClearZLimit
                    && unit.Position.Z >= _zoneToClearPosition.Z - _zoneToClearZLimit
                    && unit.IsAttackable
                    && unit.Target <= 0)
                .Where(unit => unit.PositionWithoutType.DistanceTo(_zoneToClearPosition) <= _zoneToClearRadius)
                .ToList();

            // if an enemy is in the safe spot
            IWoWUnit enemyClosestFromSafeSpot = _entityCache.EnemiesAttackingGroup
                .OrderBy(e => e.PositionWithoutType.DistanceTo(_safeSpotCenter))
                .FirstOrDefault();
            if (enemyClosestFromSafeSpot != null 
                && PositionInSafeSpotFightRange(enemyClosestFromSafeSpot.PositionWithoutType))
            {
                Logger.Log($"{enemyClosestFromSafeSpot.Name} is in the safe zone. Attacking.");
                Fight.StartFight(enemyClosestFromSafeSpot.Guid);
                return;
            }

            // Attack standing still enemy (last)
            _enemiesStandingStill = _entityCache.EnemiesAttackingGroup
                .Where(e => _pulledEnemiesDic.ContainsKey(e.Guid) && _pulledEnemiesDic[e.Guid].ShouldBeAttacked
                    && !e.IsDead
                    && e.WowUnit.IsAttackable
                    && !e.WowUnit.NotSelectable)
                .OrderBy(e => _safeSpotCenter.DistanceTo(e.PositionWithoutType))
                .ToList();
            if (_enemiesStandingStill.Count > 0)
            {
                IWoWUnit standingStillToAttack = _enemiesStandingStill.First();
                Logger.Log($"{standingStillToAttack.Name} is standing still. Attacking.");
                Fight.StartFight(standingStillToAttack.Guid);
                return;
            }

            Logger.LogOnce($"There are {enemiesToPull.Count} enemies left in the zone to clear");

            // There are enemies in the safe spot or we are attacked
            if (enemiesToPull.Count > 0 || _entityCache.EnemiesAttackingGroup.Length > 0)
            {
                // Tank logic
                if (_entityCache.IAmTank)
                {
                    if (_entityCache.EnemiesAttackingGroup.Length <= 0)
                    {
                        // record path distances
                        foreach (WoWUnit enemy in enemiesToPull)
                        {
                            if (_enemiesToClear.TryGetValue(enemy.Guid, out List<Vector3> savedPath)) // readjust paths for patrols
                            {
                                if (savedPath == null || savedPath.Last().DistanceTo(enemy.Position) > 5f)
                                {
                                    _enemiesToClear.Remove(enemy.Guid);
                                    List<Vector3> pathToEnemy = PathFinder.FindPath(_safeSpotCenter, enemy.Position);
                                    _enemiesToClear.Add(enemy.Guid, pathToEnemy);
                                    Logger.Log($"Adjusted {enemy.Name} path (enemy is patroling)");
                                }
                            }
                            else // add path to dic
                            {
                                List<Vector3> pathToEnemy = PathFinder.FindPath(_safeSpotCenter, enemy.Position);
                                _enemiesToClear.Add(enemy.Guid, pathToEnemy);
                                Logger.Log($"Added {enemy.Name} path with path distance {WTPathFinder.CalculatePathTotalDistance(pathToEnemy)} yards");
                            }
                        }
                    }

                    KeyValuePair<ulong, List<Vector3>> closestEntry = _enemiesToClear
                        .Where(kvp => kvp.Value != null)
                        .OrderBy(kvp => WTPathFinder.CalculatePathTotalDistance(kvp.Value))
                        .FirstOrDefault();

                    // Get closest enemy to pull from dictionary
                    if (closestEntry.Key > 0)
                    {
                        WoWUnit unit = enemiesToPull
                            .Where(unit => unit.Guid == closestEntry.Key)
                            .FirstOrDefault();

                        // unit is absent or dead, remove from dictionary
                        if (unit == null || unit.IsDead)
                        {
                            _enemiesToClear.Remove(closestEntry.Key);
                            return;
                        }

                        // No enemy attacking us, start pulling
                        if (_entityCache.EnemiesAttackingGroup.Length <= 0
                            || !_entityCache.Me.InCombatFlagOnly)
                        {
                            List<IWoWPlayer> myTeamMates = _entityCache.ListGroupMember
                                .Where(m => m.Guid != _entityCache.Me.Guid)
                                .ToList();
                            bool teammatesAtSafeSpot = myTeamMates.All(m => m.PositionWithoutType.DistanceTo(_safeSpotCenter) < 5f);

                            // Stop to pull
                            if (teammatesAtSafeSpot
                                && unit.Position.DistanceTo(myPos) < 50)
                            {
                                Logger.Log($"Pulling {unit.Name}");
                                Fight.StartFight(unit.Guid);
                                return;
                            }

                            // Move towards enemy to pull
                            if (!MovementManager.InMovement
                                && teammatesAtSafeSpot)
                            {
                                Logger.Log($"Pulling {unit.Name} to safe spot");
                                MovementManager.Go(WTPathFinder.PathFromClosestPoint(closestEntry.Value));
                            }
                        }
                        else
                        {
                            // Run back to safe spot
                            if (myPos.DistanceTo(_safeSpotCenter) > 2f
                                && (!MovementManager.InMovement || MovementManager.CurrentPath.Last() != _safeSpotCenter))
                            {
                                MovementManager.StopMove();
                                Logger.Log($"{_entityCache.EnemiesAttackingGroup.Count()} enemies attacking, returning to safe spot");
                                List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, _safeSpotCenter);
                                MovementManager.Go(WTPathFinder.PathFromClosestPoint(WTPathFinder.PathFromClosestPoint(pathToSafeSpot)));
                            }
                        }
                    }
                }
                else // Follower logic
                {
                    // No enemy pulled, regroup
                    if (myPos.DistanceTo(_safeSpotCenter) > 2f
                        && !MovementManager.InMovement)
                    {
                        Logger.Log($"Moving to safe spot");
                        List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, _safeSpotCenter);
                        MovementManager.Go(pathToSafeSpot);
                    }
                }
            }
            // No more enemy detected in the zone and party not in fight
            else
            {
                // Move back to spot
                if (myPos.DistanceTo(_safeSpotCenter) > 3f
                    && !MovementManager.InMovement)
                {
                    Logger.Log($"Moving to safe spot");
                    List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, _safeSpotCenter);
                    MovementManager.Go(pathToSafeSpot);
                    return;
                }

                // Complete condition
                if (EvaluateCompleteCondition()
                    && _entityCache.EnemiesAttackingGroup.Length <= 0
                    && _entityCache.ListGroupMember.All(m => !m.InCombatFlagOnly && m.PositionWithoutType.DistanceTo(_safeSpotCenter) < 5f))
                {
                    Logger.Log($"Checking complete condition");
                    _enemiesToClear.Clear();
                    IsCompleted = true;
                }
            }
        }

        private void DrawEventPathManager()
        {
            Radar3D.DrawCircle(_safeSpotCenter, _safeSpotRadius, Color.Green, false, 50);
            Radar3D.DrawCircle(_safeSpotCenter, 1, Color.Green, true, 50);
            Radar3D.DrawCircle(_zoneToClearPosition, _zoneToClearRadius, Color.Red, false, 50);
            Radar3D.DrawCircle(_zoneToClearPosition, 1, Color.Red, true, 50);
            foreach (IWoWUnit unit in _entityCache.EnemiesAttackingGroup)
            {
                if (PositionInSafeSpotFightRange(unit.PositionWithoutType))
                {
                    Radar3D.DrawLine(_entityCache.Me.PositionWithoutType, unit.PositionWithoutType, Color.Purple);
                    Radar3D.DrawCircle(unit.PositionWithoutType, 0.5f, Color.Purple, true, 100);
                }
                else
                {
                    Radar3D.DrawLine(_entityCache.Me.PositionWithoutType, unit.PositionWithoutType, Color.White);
                    Radar3D.DrawCircle(unit.PositionWithoutType, 0.5f, Color.White, true, 100);
                }
            }

            foreach (IWoWUnit unit in _enemiesStandingStill)
            {
                Radar3D.DrawLine(_entityCache.Me.PositionWithoutType, unit.PositionWithoutType, Color.Yellow);
                Radar3D.DrawCircle(unit.PositionWithoutType, 0.7f, Color.Yellow, false, 100);
            }
        }

        private class PulledEnemy
        {
            public IWoWUnit Unit;
            private Vector3 _lastRecordedPosition;
            private Timer _recordTimer;
            private int _standingStillOccurrences;
            private int _maxSecsStandingStill = 5;

            public bool ShouldBeAttacked =>_standingStillOccurrences > _maxSecsStandingStill;

            public PulledEnemy(IWoWUnit unit)
            {
                Unit = unit;
            }

            public void RecordPosition(Vector3 position)
            {
                if (!ShouldBeAttacked
                    && Unit != null
                    && !Unit.IsDead
                    && Unit.WowUnit.IsAttackable
                    && !Unit.WowUnit.NotSelectable)
                {
                    if (_recordTimer == null || _recordTimer.IsReady)
                    {
                        if (_lastRecordedPosition == position)
                        {
                            _standingStillOccurrences++;
                            Logger.Log($"{Unit.Name} has been standing still for {_standingStillOccurrences}s");
                        }
                        else
                        {
                            _standingStillOccurrences = 0;
                            _lastRecordedPosition = position;
                        }
                        _recordTimer = new Timer(1000);
                    }
                }
            }
        }
    }
}
