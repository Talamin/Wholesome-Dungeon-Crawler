using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    public interface IPathManager : ICycleable
    {
        void SetNextNode(Vector3 nextNode);
        void SetNeighboringNodes(List<Vector3> neighboringNodes);
    }
}
