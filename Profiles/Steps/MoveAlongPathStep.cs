using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveAlongPathStep : Step
    {
        private MoveAlongPathModel _moveAlongPathModel;
        private readonly IEntityCache _entityCache;
        private readonly IPathManager _pathManager;
        private Vector3 _nextNode;

        public override string Name { get; }
        public override int Order { get; }

        public List<Vector3> GetMoveAlongPath => _moveAlongPathModel.Path;

        public MoveAlongPathStep(MoveAlongPathModel stepModel, IEntityCache entityCache, IPathManager pathManager)
        {
            _moveAlongPathModel = stepModel;
            _entityCache = entityCache;
            _pathManager = pathManager;
            Name = stepModel.Name;
            Order = stepModel.Order;
        }

        public override void Run()
        {
            if (GetMoveAlongPath.Count <= 0)
            {
                Logger.LogError($"Step {Name} path is empty, skipping.");
                IsCompleted = true;
                return;
            }

            Vector3 lastPointOfPath = GetMoveAlongPath.Last();

            if (_entityCache.Me.PositionWithoutType.DistanceTo(lastPointOfPath) < 5f
                && EvaluateCompleteCondition(_moveAlongPathModel.CompleteCondition))
            {
                IsCompleted = true;
                return;
            }

            if (!MovementManager.InMovement && !_entityCache.Me.Dead)
            {
                if (_nextNode != null)
                {
                    // Rejoin previous path around last visited node
                    List<Vector3> neighboringNodes = MoveHelper.GetNodesAround(GetMoveAlongPath, _nextNode);
                    _pathManager.SetNeighboringNodes(neighboringNodes);
                    List<Vector3> orderedNeighbors = neighboringNodes
                        .OrderBy(node => node.DistanceTo(_entityCache.Me.PositionWithoutType))
                        .ToList();
                    Vector3 nodeToReach;
                    if (orderedNeighbors.Count > 1 && neighboringNodes.IndexOf(orderedNeighbors[1]) > neighboringNodes.IndexOf(orderedNeighbors[0]))
                    {
                        nodeToReach = orderedNeighbors[1];
                    }
                    else
                    {
                        nodeToReach = orderedNeighbors[0];
                    }
                    List<Vector3> adjustedPath = GetMoveAlongPath.ToList();
                    int nodeToReachIndex = GetMoveAlongPath.IndexOf(nodeToReach);
                    if (nodeToReachIndex > 0)
                    {
                        adjustedPath.RemoveRange(0, nodeToReachIndex);
                    }
                    List<Vector3> finalPath = PathFinder.FindPath(nodeToReach);
                    finalPath.AddRange(adjustedPath);
                    MovementManager.Go(finalPath);
                }
                else
                {
                    // Default join path
                    List<Vector3> pathToFollow = WTPathFinder.PathFromClosestPoint(GetMoveAlongPath);
                    MovementManager.Go(pathToFollow);
                }
            }
            else
            {
                // We're in movement, Record Next Node
                Vector3 currentMoveTo = MovementManager.CurrentMoveTo;
                if (currentMoveTo != _nextNode && GetMoveAlongPath.Contains(currentMoveTo))
                {
                    _nextNode = currentMoveTo;
                    _pathManager.SetNextNode(_nextNode);
                }
            }
        }
    }
}
