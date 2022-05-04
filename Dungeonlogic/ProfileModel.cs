using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    class ProfileModel : IProfile
    {
        public int MapId { get; private set; }
        public int DungeonId { get; private set; }
        public object Start { get; private set; }
        public Vector3 EntranceLoc { get; private set; }
        public List<Step> Steps { get; set; }
        public Dungeon Dungeon { get; private set; }
        public string Name { get; private set; }
        public string CurrentState { get; private set; }
        public bool OverrideNeedToRun { get; private set; }
        public string CurrentStepType { get; private set; }
        public Step CurrentStep { get; private set; }
        public ProfileModel CurrentProfile { get; private set; }


        public ProfileModel()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
