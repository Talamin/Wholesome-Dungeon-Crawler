using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    public interface IPathManager : ICycleable
    {
        void SetCurrentProfilePath(List<Vector3> path);
        Vector3 NextPathNode { get; }
    }
}
