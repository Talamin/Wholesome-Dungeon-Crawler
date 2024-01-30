using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class AvoidAOE : State
    {
        public override string DisplayName => "Escaping from AOE damage";

        private readonly IAvoidAOEManager _avoidAOEManager;
        private readonly IEntityCache _entityCache;
        private readonly ICache _cache;

        private RepositionInfo _repositionInfo;
        private List<Vector3> _escapePath;
        private List<BannedSafeZone> _bannedSafeZones = new List<BannedSafeZone>();
        Timer _banStateTimer = new Timer();

        public AvoidAOE(
            IEntityCache entityCache,
            IAvoidAOEManager avoidAOEManager,
            ICache cache)
        {
            _entityCache = entityCache;
            _avoidAOEManager = avoidAOEManager;
            _cache = cache;
        }

        public void Initialize()
        {
            MovementEvents.OnSeemStuck += SeemStuckHandler;
        }

        public void Dispose()
        {
            MovementEvents.OnSeemStuck -= SeemStuckHandler;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_cache.IsInInstance
                    || _entityCache.Me.IsDead
                    || !_banStateTimer.IsReady)
                {
                    return false;
                }

                // If we receive a RepositionInfo from the manager, it means we need to reposition
                _repositionInfo = _avoidAOEManager.RepositionInfo;
                if (_repositionInfo != null)
                {
                    return true;
                }
                else
                {
                    _escapePath = null;
                }
                return false;
            }
        }

        public override void Run()
        {
            Vector3 myPos = _entityCache.Me.PositionWT;
            _bannedSafeZones.RemoveAll(bsz => bsz.ShouldBeRemoved);
            List<Vector3> currentPath = new List<Vector3>(MovementManager.CurrentPath);

            // We are repositioning
            if (_escapePath != null)
            {
                if (!MovementManager.InMovement)
                {
                    MovementManager.Go(_escapePath);
                    return;
                }

                // We are not on the right path, cancel
                if (currentPath.Last() != _escapePath.Last())
                {
                    MovementManager.StopMove();
                    Logger.Log($"Canceled path because it's not our escape route");
                    MovementManager.Go(_escapePath);
                    return;
                }
                else
                {
                    if (myPos.DistanceTo(currentPath.Last()) < 3f)
                    {
                        Logger.Log($"Safe spot reached.");
                        MovementManager.StopMove();
                        _escapePath = null;
                    }
                }
                return;
            }

            Vector3 referenceGridPosition = myPos;
            if (!_repositionInfo.InSafeZone)
            {
                referenceGridPosition = _repositionInfo.ForcedSafeZone.ZoneCenter;
                Logger.LogOnce($"Trying to reposition into safe zone");
            }
            else
            {
                Logger.LogOnce($"Trying to escape {_repositionInfo.CurrentDangerZone.Name}");
            }

            Stopwatch gridWatch = Stopwatch.StartNew();
            List<Vector3> safeSpots = new List<Vector3>();
            int nbSpotsFound = 0;
            int nbSpotsInDangerZone = 0;
            int nbSpotsTooCloseToEnemy = 0;
            int nbSpotsOutsideFSZ = 0;
            int range = 30;
            int stepSize = 5;
            List<DangerZone> dangerZones = new List<DangerZone>(_repositionInfo.DangerZones);

            for (int y = -range; y <= range; y += stepSize)
            {
                for (int x = -range; x <= range; x += stepSize)
                {
                    Vector3 gridPosition = referenceGridPosition + new Vector3(x, y, 0);
                    if (dangerZones.Any(dangerZone => dangerZone.PositionInDangerZone(gridPosition, stepSize + _repositionInfo.CurrentDangerZone.ExtraMargin)))
                    {
                        nbSpotsInDangerZone++;
                        continue;
                    }
                    // don't go towards unpulled enemies
                    var closeEnemy = _entityCache.EnemyUnitsList.FirstOrDefault(enemy => !enemy.IsAttackingMe && !enemy.IsAttackingGroup && !enemy.InCombatFlagOnly && enemy.TargetGuid <= 0 && gridPosition.DistanceTo(enemy.PositionWT) < 30 && enemy.Entry != 29573 && enemy.Entry != 29830);
                    if (closeEnemy != null)
                    {
                        Logger.LogOnce($"Enemy is blocking this spot ({closeEnemy.Name})");
                        nbSpotsTooCloseToEnemy++;
                        continue;
                    }

                    if (_repositionInfo.ForcedSafeZone != null && !_repositionInfo.ForcedSafeZone.PositionInSafeZone(gridPosition, -4))
                    {
                        nbSpotsOutsideFSZ++;
                        continue;
                    }

                    if (_bannedSafeZones.Any(bsz => bsz.Position.DistanceTo(gridPosition) < 3))
                    {
                        continue;
                    }

                    nbSpotsFound++;
                    safeSpots.Add(gridPosition);
                }
            }

            Logger.LogOnce($"Spots found: {nbSpotsFound} - In danger zone: {nbSpotsInDangerZone} - Enemy too close: {nbSpotsTooCloseToEnemy} - Outside forced safe zone {nbSpotsOutsideFSZ}");

            if (safeSpots.Count <= 0)
            {
                Logger.LogError("Failed to find any safe spot!");
                _banStateTimer = new Timer(5 * 1000);
                return;
            }
            Stopwatch posWatch = Stopwatch.StartNew();
            // Prefer a position nearby me and target
            ICachedWoWUnit target = _entityCache.Target;
            ICachedWoWPlayer tank = _entityCache.TankUnit;
            Vector3 midPoint = myPos;
            if (target != null)
            {
                Vector3 targetPos = target.PositionWT;
                midPoint = new Vector3((myPos.X + targetPos.X) / 2, (myPos.Y + targetPos.Y) / 2, (myPos.Z + targetPos.Z) / 2);
            }
            List<Vector3> closestSpotsFromPreferred = safeSpots
                .OrderBy(spot => myPos.DistanceTo(spot))
                .ToList();
            foreach (Vector3 spot in closestSpotsFromPreferred)
            {
                // Should always be in range of tank
                if (tank != null && (tank.PositionWT.DistanceTo(spot) > 35))
                {
                    continue;
                }

                Vector3 spotPosition = new Vector3(spot.X, spot.Y, PathFinder.GetZPosition(spot));

                // Check LoS with tank
                if (tank != null && TraceLine.TraceLineGo(spotPosition + new Vector3(0, 0, 2), tank.PositionWT + new Vector3(0, 0, 2)))
                {
                    continue;
                }

                float straightLineDistance = myPos.DistanceTo(spotPosition);
                List<Vector3> pathToSafeSpot = PathFinder.FindPath(myPos, spotPosition, out bool foundPath, true);
                if (foundPath)
                {
                    // Avoid big detours or fall off cliffs
                    if (WTPathFinder.CalculatePathTotalDistance(pathToSafeSpot) > straightLineDistance * 1.6)
                    {
                        continue;
                    }

                    Logger.Log($"Found a path in {posWatch.ElapsedMilliseconds}ms");
                    _escapePath = pathToSafeSpot;
                    MovementManager.Go(_escapePath, false);
                    return;
                }
            }
            Logger.LogError($"No escape route found in {posWatch.ElapsedMilliseconds}ms!");
            _banStateTimer = new Timer(5 * 1000);
        }

        private void SeemStuckHandler()
        {
            // Sometimes the pathfinder will make a path through a wall
            // Need to detect when we're stuck, but not in place (in case of root spell)
            List<Vector3> currentPath = MovementManager.CurrentPath;
            if (_escapePath != null
                && currentPath != null
                && currentPath.Count > 0
                && !_entityCache.Me.WowUnit.Rooted
                && !_entityCache.Me.WowUnit.IsStunned)
            {
                Logger.LogError($"We're stuck, trying another path");
                _bannedSafeZones.Add(new BannedSafeZone(MovementManager.CurrentPath.Last()));
                _escapePath = null;
            }
        }
    }
}
