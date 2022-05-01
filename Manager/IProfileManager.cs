using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Manager
{
    interface IProfileManager : ICycleable
    {
        public bool actualDungeonProfile { get; }
        public Dungeon actualDungeon { get; }
    }
}
