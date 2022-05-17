using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Data.Model
{
    public class DungeonModel
    {
        public string Name { get; set; }
        public int MapId { get; set; }
        public int DungeonId { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 EntranceLoc { get; set; }

    }
}
