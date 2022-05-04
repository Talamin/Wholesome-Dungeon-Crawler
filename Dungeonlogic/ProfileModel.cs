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
        public int MapId { get; }
        public int DungeonId { get; }
        public object Start { get; }
        public Vector3 EntranceLoc { get; }
        public List<Step> Steps { get;}
        public Dungeon Dungeon { get; }
        public string Name { get;  }
        public string CurrentState { get; }
        public bool OverrideNeedToRun { get; }
        public string CurrentStepType { get;  }
        public Step CurrentStep { get; }
    }
}
