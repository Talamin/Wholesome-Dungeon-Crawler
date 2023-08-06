using System.Collections.Generic;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IAvoidAOEManager : ICycleable
    {
        RepositionInfo RepositionInfo { get; }

        bool CheckSpells(string caster, string sourceName, int spellId, string spellName, List<string> args);
    }
}
