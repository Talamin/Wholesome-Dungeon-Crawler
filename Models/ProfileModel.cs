using robotManager.Helpful;
using System.Collections.Generic;
using wManager.Wow.Class;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Models
{
    public class ProfileModel
    {
        public List<StepModel> StepModels { get; set; }
        public int MapId { get; set; }
        public string ProfileName { get; set; }
        public string DungeonName { get; set; }
        public Npc.FactionType Faction { get; set; }
        public List<Vector3> DeathRunPath { get; set; }
        public List<PathFinder.OffMeshConnection> OffMeshConnections { get; set; }
    }
}
