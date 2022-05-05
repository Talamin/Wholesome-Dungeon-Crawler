using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    interface IProfile
    {
        public int MapId { get; }
        public int DungeonId { get; }
        public object Start { get; }
        public Vector3 EntranceLoc { get; }
        public List<StepModel> Steps { get; }
        public Dungeon Dungeon { get; }
        public string Name { get; }
        public string CurrentState { get; }
        public bool OverrideNeedToRun { get; }

        public string CurrentStepType { get; }
        public ProfileModel CurrentProfile { get; }

        public void ExecuteSteps();

        public void Initialize(Profile profile);
        public void Dispose();
    }
}
