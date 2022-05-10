using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Profiles;

namespace WholesomeDungeonCrawler.Manager
{
    interface IProfileManager : ICycleable
    {

        public Profile CurrentDungeonProfile { get; }
    }
}
