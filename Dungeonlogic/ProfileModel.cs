using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    class ProfileModel
    {
        public List<Step> Steps { get;}
        public Dungeon Dungeon { get; }
        public string Name { get;  }
        public string CurrentState { get; }
        public bool OverrideNeedToRun { get; }
    }
}
