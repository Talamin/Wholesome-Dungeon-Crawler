using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data
{
    interface IWoWUnit
    {
        ulong Guid { get; }
        ulong TargetGuid { get; }
        string Name { get; }
        bool Valid { get; }
        bool Alive { get; }
        Vector3 PositionWithoutType { get; }
        int Reaction { get; } 
    }
}
