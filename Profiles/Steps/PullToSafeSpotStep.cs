using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class PullToSafeSpotStep : Step
    {
        private readonly PullToSafeSpotModel _pullToSafeSpotModel;
        private readonly IEntityCache _entityCache;
        private readonly Vector3 _safeSpot;
        private readonly int _safeSpotRadius;
        private readonly Vector3 _zoneToClear;
        private readonly int _zoneToClearRadius;
        private readonly int _zoneToClearZLimit;
        private readonly Dictionary<ulong, List<Vector3>> _enemiesToClear = new Dictionary<ulong, List<Vector3>>(); // unit -> path to unit from safespot
        private readonly float _myFightRange;

        public override string Name { get; }
        public bool IamInSafeSpot => _safeSpot != null && _safeSpot.DistanceTo(_entityCache.Me.PositionWithoutType) < _safeSpotRadius;
        public bool PositionInSafeSpotFightRange(Vector3 position) => position.DistanceTo(_safeSpot) < _safeSpotRadius + _myFightRange;

        public PullToSafeSpotStep(
            PullToSafeSpotModel pullToSafeSpotModel,
            IEntityCache entityCache) : base(pullToSafeSpotModel.CompleteCondition)
        {
            Name = pullToSafeSpotModel.Name;
            _pullToSafeSpotModel = pullToSafeSpotModel;
            _entityCache = entityCache;
            _safeSpotRadius = pullToSafeSpotModel.SafeSpotRadius;
            _safeSpot = pullToSafeSpotModel.SafeSpotPosition;
            _zoneToClear = pullToSafeSpotModel.ZoneToClearPosition;
            _zoneToClearRadius = pullToSafeSpotModel.ZoneToClearRadius;
            _zoneToClearZLimit = pullToSafeSpotModel.ZoneToClearZLimit;

            if (_safeSpot == null)
            {
                Logger.LogError($"ERROR: The safe spot of your current step {Name} is null!");
            }
            if (_zoneToClear == null)
            {
                Logger.LogError($"ERROR: The zone to clear position of your current step {Name} is null!");
            }

            _myFightRange = 
                WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.RDPS 
                || WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Heal
                || _entityCache.IAmTank ?
                pullToSafeSpotModel.DEFAULT_RANGED_FIGHT_RANGE
                : pullToSafeSpotModel.DEFAULT_MELEE_FIGHT_RANGE;
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
            List<WoWUnit> enemiesInClearZone = ObjectManager.GetWoWUnitHostile()
                .Where(unit => unit.Position.Z <= _zoneToClear.Z + _zoneToClearZLimit
                    && unit.Position.Z >= _zoneToClear.Z - _zoneToClearZLimit)
                .Where(unit => unit.PositionWithoutType.DistanceTo(_zoneToClear) <= _zoneToClearRadius)
                .ToList();

            Logger.LogOnce($"There are {enemiesInClearZone.Count} enemies left in the zone to clear");

            if (enemiesInClearZone.Count > 0)
            {
                // Tank logic
                if (_entityCache.IAmTank)
                {
                    // record path distances
                    foreach (WoWUnit enemy in enemiesInClearZone)
                    {
                        if (!_enemiesToClear.ContainsKey(enemy.Guid))
                        {
                            List<Vector3> pathToEnemy = PathFinder.FindPath(_safeSpot, enemy.Position);
                            _enemiesToClear.Add(enemy.Guid, pathToEnemy);
                            Logger.Log($"Detected {enemy.Name} with path distance {WTPathFinder.CalculatePathTotalDistance(pathToEnemy)} yards");
                        }
                    }

                    KeyValuePair<ulong, List<Vector3>> closestEntry = _enemiesToClear
                        .Where(kvp => kvp.Value != null)
                        .OrderBy(kvp => WTPathFinder.CalculatePathTotalDistance(kvp.Value))
                        .FirstOrDefault();

                    if (closestEntry.Key > 0)
                    {
                        WoWUnit unit = enemiesInClearZone
                            .Where(unit => unit.Guid == closestEntry.Key)
                            .FirstOrDefault();

                        // unit is absent or dead, remove from dictionary
                        if (unit == null || unit.IsDead)
                        {
                            _enemiesToClear.Remove(closestEntry.Key);
                            return;
                        }

                        if (_entityCache.EnemiesAttackingGroup.Count() <= 0
                            || !_entityCache.Me.InCombatFlagOnly)
                        {
                            if (unit.Position.DistanceTo(myPos) < 50)
                            {
                                Fight.StartFight(unit.Guid);
                                return;
                            }

                            // Move towards enemy
                            if (!MovementManager.InMovement)
                            {
                                Logger.Log($"Pulling {unit.Name} to safe spot");                               
                                MovementManager.Go(WTPathFinder.PathFromClosestPoint(closestEntry.Value));
                            }
                        }
                        else
                        {
                            // Run back to safe spot
                            if (!MovementManager.InMovement || MovementManager.CurrentPath.Last() != _safeSpot)
                            {
                                MovementManager.StopMove();
                                Logger.Log($"{_entityCache.EnemiesAttackingGroup.Count()} enemies attacking, returning to safe spot");
                                List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, _safeSpot);
                                //List<Vector3> pathToSafeSpot = new List<Vector3>(closestEntry.Value).Reverse();
                                //pathToSafeSpot.Reverse();
                                MovementManager.Go(WTPathFinder.PathFromClosestPoint(WTPathFinder.PathFromClosestPoint(pathToSafeSpot)));
                            }
                        }
                    }
                }
                else
                // Follower logic
                {
                    if (myPos.DistanceTo(_safeSpot) > _safeSpotRadius
                        && !MovementManager.InMovement)
                    {
                        List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, _safeSpot);
                        MovementManager.Go(pathToSafeSpot);
                    }
                }
            }
            else
            {
                // Zone is clear
                if (EvaluateCompleteCondition()
                    && !_entityCache.Me.InCombatFlagOnly)
                {
                    _enemiesToClear.Clear();
                    IsCompleted = true;
                }
            }
        }
    }
}
