using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    interface IStep
    {
        bool IsCompleted { get; set; }
        bool OverrideNeedToRun { get; set; }
        string Name { get; set; }

        bool Pulse { get; set; }
    }
}
