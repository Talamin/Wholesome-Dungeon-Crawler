using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Helpers
{
    internal class MoveHelper
    {
        public static List<Vector3> GetFrontLinesOnPath(List<Vector3> path, int maxDistance = 50)
        {
            List<Vector3> result = new List<Vector3>();
            Vector3 myNextNode = MovementManager.CurrentMoveTo;
            if (!path.Contains(myNextNode))
            {
                return result;
            }
            Vector3 myPos = ObjectManager.Me.Position;
            result.Add(myPos);
            float lineToCheckDistance = 0;

            for (int i = path.IndexOf(myNextNode) - 1; i < path.Count - 1; i++)
            {
                // Ignore if too far
                if (result.Count > 2 && lineToCheckDistance > maxDistance)
                {
                    break;
                }

                result.Add(path[i + 1]);
                lineToCheckDistance += path[i].DistanceTo(path[i + 1]);
            }

            return result;
        }

        public static bool PositionIsAlongPath(Vector3 position, List<Vector3> path, float distanceFromPath = 3f)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                float positionDistanceFromLine = WTPathFinder.PointDistanceToLine(path[i], path[i + 1], position);
                if (positionDistanceFromLine < distanceFromPath)
                {
                    return true;
                }
            }
            return false;
        }

        // Gets X neighboring nodes on a path
        public static List<Vector3> GetSafeNodesAround(IEntityCache entityCache, List<Vector3> path, Vector3 baseNode, int nodeAmount = 5)
        {
            List<Vector3> result = new List<Vector3>();
            int baseNodeIndex = path.IndexOf(baseNode);

            // before node
            if (baseNodeIndex > 0)
            {
                List<Vector3> nodesBeforeBase = path.Where(node => path.IndexOf(node) <= baseNodeIndex).ToList();
                nodesBeforeBase.Reverse();
                List<Vector3> pointsBefore = Toolbox.GetPointsAlongPath(nodesBeforeBase, 5f, 30f);
                pointsBefore.Reverse();
                result.AddRange(pointsBefore);
            }

            // node itself
            result.Add(baseNode);

            // after node
            if (baseNodeIndex < path.Count - 1)
            {
                List<Vector3> nodesAfterBase = path.Where(node => path.IndexOf(node) >= baseNodeIndex).ToList();
                List<Vector3> pointsAfter = Toolbox.GetPointsAlongPath(nodesAfterBase, 5f, 30f);
                result.AddRange(pointsAfter);
            }

            if (!entityCache.IAmTank)
            {
                result.RemoveAll(node => entityCache.EnemyUnitsList.Any(unit => unit.PositionWithoutType.DistanceTo(node) < 20));
            }

            return result;
        }
    }
}
