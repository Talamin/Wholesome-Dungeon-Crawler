using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    internal class CheckPathAhead : State
    {
        public override string DisplayName { get; set; } = "Check Path Ahead";

        private readonly float _detectionRadius = 45f;
        private readonly float _detectionPathDistance = 35f;
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private readonly ICache _cache;
        private readonly IProfileManager _profileManager;
        private (ICachedWoWUnit unit, float pathDistance) _unitOnPath = (null, 0);
        private List<Vector3> _pointsAlongPathSegments = new List<Vector3>();
        private (Vector3 a, Vector3 b) _dangerTraceline = (null, null);
        private List<TraceLineResult> _losCache = new List<TraceLineResult>();
        private List<Vector3> _linesToCheck = new List<Vector3>();
        private List<Vector3> _linesAllPathsInfront = new List<Vector3>();

        public CheckPathAhead(
            IEntityCache EntityCache,
            IPartyChatManager partyChatManager,
            ICache cache,
            IProfileManager profileManager)
        {
            _entityCache = EntityCache;
            _partyChatManager = partyChatManager;
            _cache = cache;
            _profileManager = profileManager;
        }

        public void Initialize()
        {
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
            {
                if (!Radar3D.IsLaunched) Radar3D.Pulse();
                Radar3D.OnDrawEvent += DrawEventAOEPathAhead;
            }
        }

        public void Dispose()
        {
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
            {
                Radar3D.OnDrawEvent -= DrawEventAOEPathAhead;
                Radar3D.Stop();
            }
        }

        public override bool NeedToRun
        {
            get
            {
                _dangerTraceline = (null, null);
                _linesToCheck.Clear();
                _pointsAlongPathSegments.Clear();
                _unitOnPath = (null, 0);

                if (_entityCache.EnemiesAttackingGroup.Length > 0
                    || !_cache.IsInInstance
                    || !_profileManager.ProfileIsRunning
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0)
                {
                    return false;
                }

                Stopwatch watch = Stopwatch.StartNew();

                // Only check path during move along step
                if (_profileManager.CurrentDungeonProfile.CurrentStep is MoveAlongPathStep moveAlongStep
                    && !moveAlongStep.IgnoreFightsDuringPath)
                {
                    List<Vector3> moveAlongPath = moveAlongStep.GetMoveAlongPath;
                    List<Vector3> currentMMPath = MovementManager.CurrentPath;
                    Vector3 myPos = _entityCache.Me.PositionWT;

                    // Check if we're on the profile path, if not, we could be in a recalculated path after a blockage
                    if (MovementManager.CurrentPath.Count > 2)
                    {
                        List<Vector3> pathToCheck = currentMMPath.GetRange(1, currentMMPath.Count - 2);
                        bool pathIsCurrent = false;
                        foreach (Vector3 node in pathToCheck)
                        {
                            if (moveAlongPath.Contains(node))
                            {
                                pathIsCurrent = true;
                                break;
                            }
                        }

                        // We are not on the profile path
                        if (!pathIsCurrent && myPos.DistanceTo(currentMMPath[0]) > 15)
                        {
                            MovementManager.StopMove();
                        }
                    }

                    Vector3 currentMANode = moveAlongPath.Where(node => node == MovementManager.CurrentMoveTo).FirstOrDefault();
                    if (currentMANode == null)
                    {
                        // Next node is not a MA path node
                        return false;
                    }
                    int currentMaNodeIndex = moveAlongPath.IndexOf(currentMANode);
                    if (currentMaNodeIndex > 0)
                    {
                        if (WTPathFinder.PointDistanceToLine(moveAlongPath[currentMaNodeIndex - 1], moveAlongPath[currentMaNodeIndex], _entityCache.Me.PositionWT) > 3f)
                        {
                            // Too far from move along path
                            return false;
                        }
                    }

                    _linesToCheck = MoveHelper.GetFrontLinesOnPath(currentMMPath);
                    _unitOnPath = EnemyAlongTheLine(_linesToCheck, _entityCache.EnemyUnitsList.Where(u => !wManager.wManagerSetting.IsBlackListed(u.Guid)).ToArray());

                    // TANK - Check for followers along the line in front
                    if (_entityCache.IAmTank && _unitOnPath.unit != null)
                    {
                        _linesAllPathsInfront = MoveHelper.GetFrontLinesOnPath(_profileManager.CurrentDungeonProfile.AllMoveAlongNodes, int.MaxValue);
                        foreach (ICachedWoWPlayer player in _entityCache.ListGroupMember)
                        {
                            if (MoveHelper.PositionIsAlongPath(player.PositionWT, _linesAllPathsInfront)
                                && myPos.DistanceTo(player.PositionWT) >= 10)
                            {
                                Logger.LogOnce($"{player.Name} is ahead. Forcing path");
                                return false;
                            }
                        }
                    }

                    // FOLLOWER - Check for tank along the line in front
                    if (!_entityCache.IAmTank && _entityCache.TankUnit != null)
                    {
                        Vector3 tankPos = _entityCache.TankUnit.PositionWT;

                        if (_entityCache.Me.PositionWT.DistanceTo(tankPos) < 10
                            && moveAlongStep.GetMoveAlongPath.Last().DistanceTo(myPos) > 15)
                        {
                            // We're next to the tank. stop
                            return true;
                        }

                        if (_unitOnPath.unit != null)
                        {
                            _linesAllPathsInfront = MoveHelper.GetFrontLinesOnPath(_profileManager.CurrentDungeonProfile.AllMoveAlongNodes, int.MaxValue);
                            if (MoveHelper.PositionIsAlongPath(tankPos, _linesAllPathsInfront))
                            {
                                // Tank is along the front line, forcing path
                                Logger.LogOnce($"The tank is along the front path, forcing my way");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    // Not a move along step
                    return false;
                }

                if (watch.ElapsedMilliseconds > 100)
                    Logger.LogError($"Check path ahead took {watch.ElapsedMilliseconds} ms");

                return _unitOnPath.unit != null;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();

            if (_entityCache.IAmTank)
            {
                if (!MyTeamIsAround)
                {
                    Logger.LogOnce($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the team to regroup.");
                }
                else
                {
                    Logger.LogOnce($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Engaging fight.");
                    Fight.StartFight(_unitOnPath.unit.Guid);
                }
            }
            else
            {
                if (_entityCache.TankUnit != null)
                {
                    if (_unitOnPath.unit != null)
                        Logger.LogOnce($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the tank.");
                    else
                        Logger.LogOnce($"Waiting for the tank to be along the path line.");
                }
                else
                {
                    Logger.LogOnce($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). I don't know where the tank is.");
                }
            }
        }

        private (ICachedWoWUnit unit, float pathDistance) EnemyAlongTheLine(List<Vector3> path, ICachedWoWUnit[] hostileUnits)
        {
            _pointsAlongPathSegments = Toolbox.GetPointsAlongPath(path, 3f, float.MaxValue);
            List<ulong> unreachableMobsGuid = new List<ulong>();
            float pathToUnitLength = 0;

            for (int i = 0; i < _pointsAlongPathSegments.Count - 1; i++)
            {
                Vector3 segmentStart = _pointsAlongPathSegments[i];
                Vector3 segmentEnd = _pointsAlongPathSegments[i + 1];
                float segmentLength = segmentStart.DistanceTo(segmentEnd);

                // check if units have LoS/path from point
                foreach (ICachedWoWUnit unit in hostileUnits)
                {
                    if ((unit.Reaction >= Reaction.Neutral && !Lists.NeutralsToAttackDuringPathCheck.Contains(unit.Entry))
                        || Lists.MobsToIgnoreDuringSteps.Contains(unit.Entry)
                        || unreachableMobsGuid.Contains(unit.Guid)
                        || unit.PositionWT.DistanceTo(_entityCache.Me.PositionWT) > _detectionRadius // in radius?
                        || pathToUnitLength + segmentStart.DistanceTo(unit.PositionWT) > _detectionPathDistance // not too far?
                        || WTPathFinder.PointDistanceToLine(segmentStart, segmentEnd, unit.PositionWT) > 20)
                    {
                        continue;
                    }

                    // Check if we already have a positive result for this unit in the cache
                    TraceLineResult positiveUnitLoS = _losCache.Where(result =>
                            result.Unit.Guid == unit.Guid
                            && result.IsVisibleAndReachable
                            && unit.PositionWT.DistanceTo(result.End) < 3f // double check for patrols
                            && segmentLength + result.Distance < _detectionPathDistance)
                        .FirstOrDefault();
                    if (positiveUnitLoS != null)
                    {
                        _dangerTraceline = (segmentStart, positiveUnitLoS.Unit.PositionWT);
                        return (positiveUnitLoS.Unit, segmentLength + positiveUnitLoS.Distance);
                    }

                    // Check the cache for any result for this traceline, cache it if not existant
                    TraceLineResult losResult = _losCache
                        .Where(tsResult => tsResult.Start.DistanceTo(segmentStart) < 3f && tsResult.End.DistanceTo(unit.PositionWT) < 3f)
                        .FirstOrDefault();
                    if (losResult == null)
                    {
                        losResult = new TraceLineResult(segmentStart, unit.PositionWT, unit);
                        _losCache.Add(losResult);

                        if (losResult.PathLength <= 0 && !unreachableMobsGuid.Contains(unit.Guid))
                        {
                            unreachableMobsGuid.Add(unit.Guid);
                        }

                        if (_losCache.Count > 100)
                        {
                            _losCache.RemoveRange(0, 20);
                        }
                    }

                    if (losResult.IsVisibleAndReachable)
                    {
                        pathToUnitLength += losResult.PathLength;
                        if (pathToUnitLength < _detectionPathDistance)
                        {
                            _dangerTraceline = (segmentStart, unit.PositionWT);
                            return (unit, pathToUnitLength);
                        }
                    }
                }

                pathToUnitLength += segmentLength;
            }

            return (null, 0);
        }

        private class TraceLineResult
        {
            public Vector3 Start;
            public Vector3 End;
            public bool HasLoS;
            public List<Vector3> Path;
            public float PathLength;
            public float Distance;
            public ICachedWoWUnit Unit;

            public TraceLineResult(Vector3 start, Vector3 end, ICachedWoWUnit unit)
            {
                Unit = unit;
                Start = start;
                End = end;
                HasLoS = !TraceLine.TraceLineGo(start, end, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                //HasLoS = Toolbox.CheckLos(start, end, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                if (!HasLoS)
                    return;
                Path = PathFinder.FindPath(start, end, out bool resultSuccess, skipIfPartiel: true);
                PathLength = resultSuccess ? WTPathFinder.CalculatePathTotalDistance(Path) : 0;
                Distance = start.DistanceTo(end);
            }

            public bool IsVisibleAndReachable => PathLength > 0
                && HasLoS
                && PathLength < Distance * 2;
        }

        private bool MyTeamIsAround => _entityCache.ListGroupMember.Length == _entityCache.ListPartyMemberNames.Count
                    && _entityCache.ListGroupMember.All(member => member.PositionWT.DistanceTo(_entityCache.Me.PositionWT) < 15);

        private void DrawEventAOEPathAhead()
        {
            if (!WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar) return;

            try
            {
                if (_unitOnPath.unit != null)
                {
                    Radar3D.DrawCircle(_unitOnPath.unit.PositionWT, 0.4f, Color.Red, true, 200);
                }

                List<Vector3> linesToCheck = new List<Vector3>(_linesToCheck);
                for (int i = 0; i < linesToCheck.Count - 1; i++)
                {
                    if (linesToCheck[i] != null && linesToCheck[i + 1] != null)
                    {
                        Radar3D.DrawLine(linesToCheck[i], linesToCheck[i + 1], Color.PaleTurquoise, 150);
                    }
                }
                /*
                foreach ((Vector3 a, Vector3 b) line in _linesAllPathsInfront)
                {
                    Radar3D.DrawLine(line.a, line.b, Color.GreenYellow, 255);
                }
                foreach ((Vector3 a, Vector3 b) line in _linesAllPathsBehind)
                {
                    Radar3D.DrawLine(line.a, line.b, Color.IndianRed, 255);
                }
                */
                List<Vector3> pointsAlongSegments = new List<Vector3>(_pointsAlongPathSegments);
                foreach (Vector3 point in pointsAlongSegments)
                {
                    Radar3D.DrawCircle(point, 0.2f, Color.Green, true, 150);
                }

                if (_dangerTraceline.a != null && _dangerTraceline.b != null)
                {
                    Radar3D.DrawCircle(_dangerTraceline.a, 0.4f, Color.Red, false, 200);
                    Radar3D.DrawLine(_dangerTraceline.a, _dangerTraceline.b, Color.Red, 200);
                }
                /*
                foreach (TraceLineResult result in _losCache)
                {
                    if (result.IsVisibleAndReachable)
                        Radar3D.DrawLine(result.Start, result.End, Color.Green, 200);
                    else
                        Radar3D.DrawLine(result.Start, result.End, Color.Red, 200);
                }*/
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }
}