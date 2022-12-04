using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private readonly float _detecttionPathDistance = 35f;
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private readonly ICache _cache;
        private readonly IProfileManager _profileManager;
        private Timer _logTimer = new Timer();
        private (IWoWUnit unit, float pathDistance) _unitOnPath = (null, 0);
        private List<(Vector3 a, Vector3 b)> _linesToCheck = new List<(Vector3 a, Vector3 b)>();
        private List<Vector3> _pointsAlongPathSegments = new List<Vector3>();
        private (Vector3 a, Vector3 b) _dangerTraceline = (null, null);
        private List<TraceLineResult> _losCache = new List<TraceLineResult>();

        public CheckPathAhead(IEntityCache EntityCache, IPartyChatManager partyChatManager, ICache cache, IProfileManager profileManager)
        {
            _entityCache = EntityCache;
            _partyChatManager = partyChatManager;
            _cache = cache;
            _profileManager = profileManager;
        }

        public void Initialize()
        {
            Radar3D.OnDrawEvent += Radar3DOnDrawEvent;
            Radar3D.Pulse();
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= Radar3DOnDrawEvent;
        }

        public override bool NeedToRun
        {
            get
            {
                _dangerTraceline = (null, null);
                _linesToCheck.Clear();
                _pointsAlongPathSegments.Clear();
                _unitOnPath = (null, 0);

                if (!_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0
                    || !Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
                {
                    return false;
                }

                // Only check path during move along step
                if (_profileManager.CurrentDungeonProfile.CurrentStep is MoveAlongPathStep)
                {
                    MoveAlongPathStep moveAlongStep = (MoveAlongPathStep)_profileManager.CurrentDungeonProfile.CurrentStep;
                    List<Vector3> moveAlongPath = moveAlongStep.GetMoveAlongPath;
                    Vector3 currentMANode = moveAlongPath.Where(node => node == MovementManager.CurrentMoveTo).FirstOrDefault();
                    if (currentMANode == null)
                    {
                        // Next node is not a MA path node
                        return false;
                    }
                    int currentMaNodeIndex = moveAlongPath.IndexOf(currentMANode);
                    if (currentMaNodeIndex > 0)
                    {
                        if (WTPathFinder.PointDistanceToLine(moveAlongPath[currentMaNodeIndex - 1], moveAlongPath[currentMaNodeIndex], _entityCache.Me.PositionWithoutType) > 3f)
                        {
                            // Too far from move along path
                            return false;
                        }
                    }
                }
                else
                {
                    // Not a move along step
                    return false;
                }

                Stopwatch watch = Stopwatch.StartNew();

                _linesToCheck = MoveHelper.GetLinesToCheckOnCurrentPath(_entityCache.Me.PositionWithoutType);
                _unitOnPath = EnemyAlongTheLine(_linesToCheck, _entityCache.EnemyUnitsList);

                if (watch.ElapsedMilliseconds > 50)
                    Logger.LogError($"Calc took {watch.ElapsedMilliseconds} ms | {_losCache.Count} in cache");

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
                    Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the team to regroup.");
                    _logTimer = new Timer(1000 * 10);
                }
                else
                {
                    Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Engaging fight.");
                    Fight.StartFight(_unitOnPath.unit.Guid);
                }
            }
            else
            {
                if (_entityCache.TankUnit != null)
                {
                    Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the tank to come.");
                    _logTimer = new Timer(1000 * 10);
                }
                else
                {
                    Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). I don't know where the tank is.");
                    /*
                    _partyChatManager.Broadcast(PartyChatManager.ChatMessageType.ASSIST_WITH_ENEMIES_AHEAD, null);
                    _logTimer = new Timer(1000 * 10);
                    */
                }
            }
        }

        private void Log(string message)
        {
            if (_logTimer.IsReady)
            {
                Logger.Log(message);
            }
        }

        private (IWoWUnit unit, float pathDistance) EnemyAlongTheLine(List<(Vector3 start, Vector3 end)> segments, IWoWUnit[] hostileUnits)
        {
            List<Vector3> pointsAlongLine = new List<Vector3>();
            List<ulong> unreachableMobsGuid = new List<ulong>();
            float pathToUnitLength = 0;
            float remainder = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 segmentStart = segments[i].start;
                Vector3 segmentEnd = segments[i].end;
                float segmentLength = segmentStart.DistanceTo(segmentEnd);

                // get points along line
                for (float offsetIndex = 3; offsetIndex < segmentLength; offsetIndex += 3)
                {
                    if (remainder > 0)
                    {
                        offsetIndex -= remainder;
                        remainder = 0;
                    }

                    if (offsetIndex + 3 > segmentLength)
                    {
                        remainder = segmentLength - offsetIndex;
                    }

                    Vector3 vector = new Vector3(segmentEnd.X - segmentStart.X, segmentEnd.Y - segmentStart.Y, segmentEnd.Z - segmentStart.Z);
                    double c = System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
                    double a = offsetIndex / c;
                    Vector3 offset = new Vector3(segmentStart.X + vector.X * a, segmentStart.Y + vector.Y * a, segmentStart.Z + vector.Z * a);

                    if (offset.DistanceTo(_entityCache.Me.PositionWithoutType) < 5)
                    {
                        continue;
                    }

                    _pointsAlongPathSegments.Add(offset);

                    // check if units have LoS/path from point
                    foreach (IWoWUnit unit in hostileUnits)
                    {
                        if (((int)unit.Reaction) > 2
                            || Lists.IgnoreWhenCheckingPathListInt.Contains(unit.UnitID)
                            || unreachableMobsGuid.Contains(unit.Guid)
                            || unit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) > _detectionRadius // in radius?
                            || pathToUnitLength + offset.DistanceTo(unit.PositionWithoutType) > _detecttionPathDistance // not too far?
                            || WTPathFinder.PointDistanceToLine(segmentStart, segmentEnd, unit.PositionWithoutType) > 20)
                        {
                            continue;
                        }

                        // Check if we already have a positive result for this unit in the cache
                        TraceLineResult positiveUnitLoS = _losCache.Where(result =>
                                result.Unit.Guid == unit.Guid
                                && result.IsVisibleAndReachable
                                && unit.PositionWithoutType.DistanceTo(result.End) < 3f // double check for patrols
                                && segmentLength + result.Distance < _detecttionPathDistance)
                            .FirstOrDefault();
                        if (positiveUnitLoS != null)
                        {
                            _dangerTraceline = (offset, positiveUnitLoS.Unit.PositionWithoutType);
                            return (positiveUnitLoS.Unit, segmentLength + positiveUnitLoS.Distance);
                        }

                        // Check the cache for any result for this traceline, cache it if not existant
                        TraceLineResult losResult = _losCache
                            .Where(tsResult => tsResult.Start.DistanceTo(offset) < 3f && tsResult.End.DistanceTo(unit.PositionWithoutType) < 3f)
                            .FirstOrDefault();
                        if (losResult == null)
                        {
                            losResult = new TraceLineResult(offset, unit.PositionWithoutType, unit);
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
                            if (pathToUnitLength < _detecttionPathDistance)
                            {
                                _dangerTraceline = (offset, unit.PositionWithoutType);
                                return (unit, pathToUnitLength);
                            }
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
            public IWoWUnit Unit;

            public TraceLineResult(Vector3 start, Vector3 end, IWoWUnit unit)
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
                    && _entityCache.ListGroupMember.All(member => member.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) < 20);

        private void Radar3DOnDrawEvent()
        {
            if (_unitOnPath.unit != null)
            {
                Radar3D.DrawCircle(_unitOnPath.unit.PositionWithoutType, 0.4f, Color.Red, true, 200);
            }

            foreach ((Vector3 a, Vector3 b) line in _linesToCheck)
            {
                Radar3D.DrawLine(line.a, line.b, Color.PaleTurquoise, 150);
            }

            foreach (Vector3 point in _pointsAlongPathSegments)
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
    }
}