using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Profiles;
using wManager.Wow.Helpers;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Models
{
    public class ProfileModel
    {
        public List<StepModel> StepModels { get; set; }
        public int MapId { get; set; }
        public string ProfileName { get; set; }
        public string DungeonName { get; set; }
        public List<DeathRun> DeathRunPaths { get; set; } = new List<DeathRun>();
        public List<PathFinder.OffMeshConnection> OffMeshConnections { get; set; }
        public DungeonModel DungeonModel { get; set; }

        public List<Vector3> DeathRunPath { get; set; } // Obsolete, only used to convert old profiles with one single deathrun
    }
}
