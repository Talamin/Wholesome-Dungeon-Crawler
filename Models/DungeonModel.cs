using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Models
{
    public class DungeonModel
    {
        public string Name { get; set; }
        public int MapId { get; set; }
        public int DungeonId { get; set; }
        public int ContinentId { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 EntranceLoc { get; set; }
    }
}
