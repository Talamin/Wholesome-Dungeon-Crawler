using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IAvoidAOEManager : ICycleable
    {
        bool ShouldReposition { get; }
        List<Vector3> GetEscapePath { get; }
    }
}
