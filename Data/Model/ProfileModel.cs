using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Data.Model
{
    class ProfileModel
    {
        public List<StepModel> StepModels { get;}
        public DungeonModel DungeonModel { get; }
        public string Name { get;  }
        public string CurrentState { get; }
        public bool OverrideNeedToRun { get; }
    }
}
