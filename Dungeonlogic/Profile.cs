using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    class Profile : IProfile
    {
        public int MapId { get; private set; }
        public int DungeonId { get; private set; }
        public object Start { get; private set; }
        public Vector3 EntranceLoc { get; private set; }
        public List<Step> Steps { get; private set; }
        public Dungeon Dungeon { get; private set; }
        public string Name { get; private set; }
        public string CurrentState { get; private set; }
        public bool OverrideNeedToRun { get; private set; }

        public string CurrentStepType { get; private set; }
        public Step CurrentStep { get; private set; }


        private readonly IProfileManager _profileManager;

        public Profile()
        {
            MapId = _profileManager.dungeonProfile.MapId;
            DungeonId = _profileManager.dungeonProfile.DungeonId;
            Start = _profileManager.dungeonProfile.Start;
            EntranceLoc = _profileManager.dungeonProfile.EntranceLoc;
            Steps = _profileManager.dungeonProfile.Steps;
            Dungeon = _profileManager.dungeonProfile.Dungeon;
            Name = _profileManager.dungeonProfile.Name;
        }

        public void ExecuteSteps()
        {
            for (int i = 0; i <Steps.Count(); i++)
            {
                var actualStep = Steps[i];
                if (CurrentStep.IsCompleted)
                {
                    Steps[i].IsCompleted = true;
                }
                if (!actualStep.IsCompleted)
                {
                    CurrentStepType = actualStep.Type;
                    CurrentStep = actualStep;
                    return;
                }
            }
        }
    }
}
