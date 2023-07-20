using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveAlongPathStep : Step
    {
        private MoveAlongPathModel _moveAlongPathModel;
        private readonly IEntityCache _entityCache;
        private readonly IPathManager _pathManager;
        private Vector3 _nextNode;
        private int _stepIndex;

        private static int CacheStepIndex = -1;
        private static bool FirstLaunch = false;

        public override string Name { get; }

        public List<Vector3> GetMoveAlongPath => _moveAlongPathModel.Path;

        public MoveAlongPathStep(
            MoveAlongPathModel stepModel,
            IEntityCache entityCache,
            IPathManager pathManager,
            int mapStepIndex) : base(stepModel.CompleteCondition)
        {
            _moveAlongPathModel = stepModel;
            _entityCache = entityCache;
            _pathManager = pathManager;
            _stepIndex = mapStepIndex;
            Name = stepModel.Name;
            FirstLaunch = true;
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (GetMoveAlongPath.Count <= 0)
            {
                Logger.LogError($"Step {Name} path is empty, skipping.");
                IsCompleted = true;
                return;
            }

            if (ObjectManager.Me.Rooted || !ObjectManager.Me.CanMove || ObjectManager.Me.IsStunned)
            {
                MovementManager.StopMove();
                Logger.LogOnce($"We're rooted or stunned, waiting");
                return;
            }

            Vector3 lastPointOfPath = GetMoveAlongPath.Last();

            if ((MovementManager.CurrentMoveTo == null || MovementManager.CurrentMoveTo == lastPointOfPath)
                && _entityCache.Me.PositionWT.DistanceTo(lastPointOfPath) < 2f
                && EvaluateCompleteCondition())
            {
                IsCompleted = true;
                return;
            }

            if (!MovementManager.InMovement && !_entityCache.Me.IsDead)
            {
                if (_nextNode != null && CacheStepIndex == _stepIndex)
                {
                    // Rejoin previous path around last visited node
                    List<Vector3> neighboringNodes = MoveHelper.GetSafeNodesAround(_entityCache, GetMoveAlongPath, _nextNode);
                    _pathManager.SetNeighboringNodes(neighboringNodes);
                    List<Vector3> orderedNeighbors = neighboringNodes
                        .OrderBy(node => node.DistanceTo(_entityCache.Me.PositionWT))
                        .ToList();
                    Vector3 neighborNodeToReach;

                    if (orderedNeighbors.Count > 1 && neighboringNodes.IndexOf(orderedNeighbors[1]) > neighboringNodes.IndexOf(orderedNeighbors[0]))
                    {
                        neighborNodeToReach = orderedNeighbors[1];
                    }
                    else
                    {
                        neighborNodeToReach = orderedNeighbors[0]; // index?
                    }

                    List<Vector3> adjustedPath = GetMoveAlongPath.ToList();

                    // il faut trouver sur quel segment se trouve neighborNodeToReach
                    bool segmentFound = false;
                    for (int i = 0; i < GetMoveAlongPath.Count - 1; i++)
                    {
                        if (WTPathFinder.PointDistanceToLine(GetMoveAlongPath[i], GetMoveAlongPath[i + 1], neighborNodeToReach) < 1f)
                        {
                            adjustedPath.RemoveRange(0, i + 1);
                            segmentFound = true;
                            break;
                        }
                    }

                    if (!segmentFound)
                    {
                        Logger.LogError($"WANING: Couldn't find neighboring node along path segments");
                        adjustedPath.RemoveRange(0, GetMoveAlongPath.IndexOf(_nextNode) + 1);
                    }

                    List<Vector3> finalPath = PathFinder.FindPath(neighborNodeToReach);
                    finalPath.AddRange(adjustedPath);
                    MovementManager.Go(finalPath);
                }
                else
                {
                    if (FirstLaunch)
                    {
                        // Default join path
                        List<Vector3> pathToFollow = WTPathFinder.PathFromClosestPoint(GetMoveAlongPath);
                        Vector3 firstPathNode = pathToFollow.Find(node => GetMoveAlongPath.Contains(node));
                        if (firstPathNode != null)
                        {
                            _nextNode = firstPathNode;
                            _pathManager.SetNextNode(firstPathNode);
                        }
                        MovementManager.Go(pathToFollow);
                    }
                    else
                    {
                        // New path, start from node 0
                        List<Vector3> pathToFollow = PathFinder.FindPath(GetMoveAlongPath[0]);
                        _nextNode = GetMoveAlongPath[0];
                        _pathManager.SetNextNode(GetMoveAlongPath[0]);
                        pathToFollow.AddRange(GetMoveAlongPath);
                        MovementManager.Go(pathToFollow);
                    }
                }

                CacheStepIndex = _stepIndex;
                FirstLaunch = false;
            }
            else
            {
                // We're in movement, Record Next Node
                Vector3 currentMoveTo = MovementManager.CurrentMoveTo;
                if (currentMoveTo != null
                    && currentMoveTo != _nextNode
                    && GetMoveAlongPath.Contains(currentMoveTo))
                {
                    _nextNode = currentMoveTo;
                    _pathManager.SetNextNode(_nextNode);
                    return;
                }

                if (_nextNode != null 
                    && GetMoveAlongPath.Contains(_nextNode))
                {
                    List<Vector3> currentPath = MovementManager.CurrentPath;
                    Vector3 nextPathNode = currentPath
                        .Where(node => GetMoveAlongPath.Contains(node)
                            && GetMoveAlongPath.IndexOf(node) >= GetMoveAlongPath.IndexOf(_nextNode))
                        .FirstOrDefault();

                    if (nextPathNode != null
                        && nextPathNode != _nextNode
                        && (nextPathNode != GetMoveAlongPath.Last() || currentPath.Count <= 3)) // avoid path recalc shenanigans after stuck
                    {
                        _nextNode = nextPathNode;
                        _pathManager.SetNextNode(_nextNode);
                        return;
                    }
                }
            }
        }
    }
}
