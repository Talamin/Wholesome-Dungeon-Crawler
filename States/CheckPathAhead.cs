using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    internal class CheckPathAhead : State
    {
        public override string DisplayName { get; set; } = "Check Path Ahead";

        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private Timer _broadcastTimer = new Timer();
        private (IWoWUnit unit, float pathDistance) _unitOnPath = (null, 0);
        private List<(Vector3 a, Vector3 b)> _linesToCheck = new List<(Vector3 a, Vector3 b)>();
        private List<Vector3> _pointsAlongPathSegments = new List<Vector3>();
        private List<(Vector3 a, Vector3 b)> _dangerTracelines = new List<(Vector3 a, Vector3 b)>();

        public CheckPathAhead(IEntityCache EntityCache, IPartyChatManager partyChatManager)
        {
            _entityCache = EntityCache;
            _partyChatManager = partyChatManager;
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
                if (!_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || _entityCache.Me.InCombatFlagOnly
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0
                    || !Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
                {
                    return false;
                }

                Stopwatch watch = Stopwatch.StartNew();

                _dangerTracelines.Clear();
                _linesToCheck.Clear();
                _pointsAlongPathSegments.Clear();
                _unitOnPath = (null, 0);
                
                _linesToCheck = MoveHelper.GetLinesToCheckOnCurrentPath(_entityCache.Me.PositionWithoutType);
                _unitOnPath = EnemyAlongTheLine(_linesToCheck, _entityCache.EnemyUnitsList);

                if (watch.ElapsedMilliseconds > 50)
                    Logger.LogError($"Calc took {watch.ElapsedMilliseconds} ms | {LosCache.Count} in cache");

                // the tank is closer from the unit, we can go
                if (_unitOnPath.unit != null
                    && !_entityCache.IAmTank
                    && _entityCache.TankUnit != null
                    && _entityCache.TankUnit.PositionWithoutType.DistanceTo(_unitOnPath.unit.PositionWithoutType) + 5 < _entityCache.Me.PositionWithoutType.DistanceTo(_unitOnPath.unit.PositionWithoutType))
                {
                    return false;
                }

                return _unitOnPath.unit != null;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();

            if (_broadcastTimer.IsReady)
            {
                if (_entityCache.IAmTank)
                {
                    if (!MyTeamIsAround)
                    {
                        Logger.Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the team to move their frail asses.");
                        _broadcastTimer = new Timer(1000 * 10);
                    }
                    else
                    {
                        Logger.Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Engaging fight.");
                        Fight.StartFight(_unitOnPath.unit.Guid);

                    }
                }
                else
                {
                    if (_entityCache.TankUnit != null)
                    {
                        Logger.Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). Waiting for the tank to move his fat ass.");
                        _broadcastTimer = new Timer(1000 * 10);
                    }
                    else
                    {
                        Logger.Log($"{_unitOnPath.unit.Name} is on the way ({_unitOnPath.pathDistance} path distance). I don't know where the tank is.");
                        _partyChatManager.Broadcast(PartyChatManager.ChatMessageType.ASSIST_WITH_ENEMIES_AHEAD, null);
                        _broadcastTimer = new Timer(1000 * 10);
                    }
                }
            }
        }


        private (IWoWUnit unit, float pathDistance) EnemyAlongTheLine(List<(Vector3 start, Vector3 end)> segments, IWoWUnit[] hostileUnits)
        {
            float distanceToUnit = 0;
            List<Vector3> pointsAlongLine = new List<Vector3>();
            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 segmentStart = segments[i].start;
                Vector3 segmentEnd = segments[i].end;
                int pointsOffset = 3;
                _pointsAlongPathSegments.Add(segmentEnd);
                float segmentLength = segmentStart.DistanceTo(segmentEnd);

                // get points along line
                for (int j = pointsOffset; j < segmentLength; j += pointsOffset)
                {
                    Vector3 vector = new Vector3(segmentEnd.X - segmentStart.X, segmentEnd.Y - segmentStart.Y, segmentEnd.Z - segmentStart.Z);
                    double c = System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
                    double a = j / c;
                    Vector3 offset = new Vector3(segmentStart.X + vector.X * a, segmentStart.Y + vector.Y * a, segmentStart.Z + vector.Z * a);
                    _pointsAlongPathSegments.Add(offset);

                    // check if units have LoS on point
                    foreach (IWoWUnit unit in hostileUnits)
                    {
                        if (WTPathFinder.PointDistanceToLine(segmentStart, segmentEnd, unit.PositionWithoutType) > 20)
                        {
                            continue;
                        }

                        TraceLineResult losResult = LosCache
                            .Where(tsResult => tsResult.Start.DistanceTo(offset) < 2 && tsResult.End.DistanceTo(unit.PositionWithoutType) < 2)
                            .FirstOrDefault();

                        if (losResult == null)
                        {
                            losResult = new TraceLineResult(offset, unit.PositionWithoutType);
                            LosCache.Add(losResult);
                            if (LosCache.Count > 300)
                            {
                                LosCache.RemoveRange(0, 50);
                            }
                        }

                        if (losResult.IsVisibleAndReachable)
                        {
                            distanceToUnit += losResult.PathLength;
                            if (distanceToUnit < 40)
                            {
                                _dangerTracelines.Add((offset, unit.PositionWithoutType));
                                return (unit, distanceToUnit);
                            }
                        }
                    }
                }
                distanceToUnit += segmentLength;
            }

            return (null, 0);
        }

        /*
        private (IWoWUnit unit, float pathDistance) EnemyAlongTheLine(List<(Vector3 a, Vector3 b)> segments, IWoWUnit[] hostileUnits)
        {
            float distanceToUnit = 0;
            foreach ((Vector3 start, Vector3 end) segment in segments)
            {
                int pointsOffset = 3;
                _pointsAlongPathSegments.Add(segment.end);
                float segmentLength = segment.start.DistanceTo(segment.end);
                
                // get points along line
                for (int i = pointsOffset; i < segmentLength; i += pointsOffset)
                {
                    Vector3 vector = new Vector3(segment.end.X - segment.start.X, segment.end.Y - segment.start.Y, segment.end.Z - segment.start.Z);
                    double c = System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
                    double a = i / c;
                    Vector3 offset = new Vector3(segment.start.X + vector.X * a, segment.start.Y + vector.Y * a, segment.start.Z + vector.Z * a);
                    _pointsAlongPathSegments.Add(offset);
                    
                    // check if units have LoS on point
                    foreach (IWoWUnit unit in hostileUnits)
                    {
                        if (WTPathFinder.PointDistanceToLine(segment.start, segment.end, unit.PositionWithoutType) > 20)
                        {
                            continue;
                        }

                        TraceLineResult losResult = LosCache
                            .Where(tsResult => tsResult.Start.DistanceTo(offset) < 2 && tsResult.End.DistanceTo(unit.PositionWithoutType) < 2)
                            .FirstOrDefault();

                        if (losResult == null)
                        {
                            losResult = new TraceLineResult(offset, unit.PositionWithoutType);
                            LosCache.Add(losResult);
                            if (LosCache.Count > 300)
                            {
                                LosCache.RemoveRange(0, 50);
                            }
                        }
                        
                        if (losResult.HasLoS)
                        {
                            distanceToUnit += segment.start.DistanceTo(offset) + offset.DistanceTo(unit.PositionWithoutType);
                            if (distanceToUnit < 40)
                            {
                                _dangerTracelines.Add((offset, unit.PositionWithoutType));
                                return (unit, distanceToUnit);
                            }
                        }
                    }
                }
                distanceToUnit += segmentLength;
            }

            return (null, 0);
        }*/

        private List<TraceLineResult> LosCache = new List<TraceLineResult>();

        private class TraceLineResult
        {
            public Vector3 Start;
            public Vector3 End;
            public bool HasLoS;
            public List<Vector3> Path;
            public float PathLength;
            public float Distance;

            public TraceLineResult(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
                HasLoS = !TraceLine.TraceLineGo(start, end, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                if (!HasLoS)
                    return;
                Path = PathFinder.FindPath(start, end, true);
                PathLength = WTPathFinder.CalculatePathTotalDistance(Path);
                Distance = start.DistanceTo(end);
            }

            public bool IsVisibleAndReachable => PathLength > 0 
                && HasLoS
                && PathLength < Distance * 2;
        }

        private bool MyTeamIsAround => _entityCache.ListGroupMember.Length == _entityCache.ListPartyMemberNames.Count
                    && _entityCache.ListGroupMember.All(member => member.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) < 40);

        private void Radar3DOnDrawEvent()
        {
            if (Fight.InFight)
                return;

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
                Radar3D.DrawCircle(point, 0.4f, Color.Green, true, 100);
            }

            foreach ((Vector3 a, Vector3 b) line in _dangerTracelines)
            {
                Radar3D.DrawCircle(line.a, 0.4f, Color.Red, true, 200);
                Radar3D.DrawLine(line.a, line.b, Color.Red, 200);
            }
        }
    }
}