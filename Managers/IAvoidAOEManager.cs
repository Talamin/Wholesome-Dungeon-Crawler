using System.Collections.Generic;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IAvoidAOEManager : ICycleable
    {
        RepositionInfo RepositionInfo { get; }

        bool CheckSpells(List<string> args);
    }
}
