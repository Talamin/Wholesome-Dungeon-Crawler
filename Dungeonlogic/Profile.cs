using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
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
        public ProfileModel CurrentProfile { get; private set; }


        public Profile()
        {
        }

        public void Initialize(ProfileModel profileModel)
        {

        }

        public void Dispose()
        {
        }

        public void ExecuteSteps()
        {
            var TotalSteps = Steps.Count();
            if (TotalSteps <= 0)
            {
                Logger.Log("Profile is missing Profile Steps.");
                return;
            }
            var IncompleteSteps = Steps.Count(s => s.IsCompleted == false);
            var CompletedSteps = Steps.Count(s => s.IsCompleted == true);
            if (CurrentStep == null)
            {
                CurrentStep = Steps[0];
            }
            if (!CurrentStep.IsCompleted)
            {
                CurrentStepType = CurrentStep.Type;
                return;
            }
            if (CurrentStep.IsCompleted)
            {
                if (CompletedSteps == TotalSteps)
                {
                    Logger.Log("Profile is Done");
                    return;
                }
                var actualstep = TotalSteps - IncompleteSteps + 1;
                CurrentStep = Steps[actualstep];
                CurrentStep.IsCompleted = false;
                CurrentStepType = CurrentStep.Type;
                return;
            }
        }
    }
}
