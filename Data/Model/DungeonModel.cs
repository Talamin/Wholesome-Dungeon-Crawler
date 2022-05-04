using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data.Model
{
    class DungeonModel
    {
        public string Name { get; }
        public int MapId { get; }
        public int DungeonId { get; }
        public Vector3 Start { get; }
        public Vector3 EntranceLoc  { get; }
}
}
