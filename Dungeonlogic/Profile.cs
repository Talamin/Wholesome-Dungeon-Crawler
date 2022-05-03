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
        public List<Step> Steps { get; set; }
        public Dungeon Dungeon { get; private set; }
        public string Name { get; private set; }
        public string CurrentState { get; private set; }
        public bool OverrideNeedToRun { get; private set; }

        public string CurrentStepType { get; private set; }
        public Step CurrentStep { get; private set; }
        public Profile CurrentProfile { get; private set; }


        private readonly IProfileManager _profileManager;

        public Profile()
        {
            CurrentProfile = _profileManager.dungeonProfile;
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
            var totalSteps = Steps.Count();
            var IncompleteSteps = Steps.Count(s => s.IsCompleted = false);
            if(CurrentStep == null)
            {
                CurrentStep = Steps[0];
            }
            if(!CurrentStep.IsCompleted)
            {
                CurrentStepType = CurrentStep.Type;
                return;
            }
            if (CurrentStep.IsCompleted)
            {
                var actualstep = totalSteps - IncompleteSteps;
                CurrentStep = Steps[actualstep];
                CurrentStep.IsCompleted = false;
                return;
            }
            Logger.Log("Profile is Done");
        }
    }
}
