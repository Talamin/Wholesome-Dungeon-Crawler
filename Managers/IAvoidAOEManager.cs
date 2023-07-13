using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.ProductCache;
using static WholesomeDungeonCrawler.Managers.AvoidAOEManager;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IAvoidAOEManager : ICycleable
    {
        bool ShouldReposition { get; }
        List<Vector3> GetEscapePath { get; }
    }
}
