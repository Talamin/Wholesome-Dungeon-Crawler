using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data
{
    interface ICache
    {
        void Initialize();
        void Dispose();
    }
}
