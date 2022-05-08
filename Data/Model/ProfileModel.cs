using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Data.Model
{
    public class ProfileModel
    {
        public List<StepModel> StepModels { get; set; }
        public int MapId { get; set; }
        public string Name { get; set; }
        public string CurrentState { get; }
        public bool OverrideNeedToRun { get; }
    }
}
