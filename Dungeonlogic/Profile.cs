using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    class Profile : IProfile
    {

        public ProfileModel ProfileModel { get; private set; }
        public StepModel CurrentStep { get; private set; }



        public Profile(ProfileModel profileModel)
        {
            ProfileModel = profileModel;
        }

        public void Dispose()
        {
        }

        public void ExecuteSteps()
        {
            var TotalSteps = ProfileModel.StepModels.Count();
            if (TotalSteps <= 0)
            {
                Logger.Log("Profile is missing Profile Steps.");
                return;
            }
            var IncompleteSteps = ProfileModel.StepModels.Count(s => s.IsCompleted == false);
            var CompletedSteps = ProfileModel.StepModels.Count(s => s.IsCompleted == true);
            if (CurrentStep == null)
            {
                CurrentStep = ProfileModel.StepModels[0];
            }
            //if (!CurrentStep.IsCompleted)
            //{
            //    CurrentStepType = CurrentStep.StepType;
            //    return;
            //}
            if (CurrentStep.IsCompleted)
            {
                if (CompletedSteps == TotalSteps)
                {
                    Logger.Log("Profile is Done");
                    return;
                }
                var actualstep = TotalSteps - IncompleteSteps + 1;
                CurrentStep = ProfileModel.StepModels[actualstep];
                CurrentStep.IsCompleted = false;
                //CurrentStepType = CurrentStep.StepType;
                return;
            }
        }
    }
}
