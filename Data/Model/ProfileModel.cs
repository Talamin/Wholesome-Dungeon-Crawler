using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using wManager.Wow.Class;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Data.Model
{
    public class ProfileModel
    {
        public List<StepModel> StepModels { get; set; }
        public int MapId { get; set; }
        public string Name { get; set; }
        public Npc.FactionType Faction { get; set; }
        public List<Vector3> DeathRunPath { get; set; }
        public List<PathFinder.OffMeshConnection> OffMeshConnections { get; set; }
    }
}
