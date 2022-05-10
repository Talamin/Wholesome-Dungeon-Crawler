using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Profiles.Steps;

namespace WholesomeDungeonCrawler.Profiles
{
    public class Profile : IProfile
    {
        private List<IStep> _profileSteps = new List<IStep>();

        public IStep CurrentStep { get; private set; }

        public Profile(ProfileModel profileModel)
        {
            foreach (StepModel model in profileModel.StepModels)
            {
                if (model is MoveAlongPathModel)
                {
                    _profileSteps.Add(new MoveAlongPathStep((MoveAlongPathModel)model));
                }
                else if (model is GoToModel)
                {
                    _profileSteps.Add(new GoToStep((GoToModel)model));
                }
                //elseif...
            }
        }

        public void Dispose()
        {
        }

        public void ExecuteSteps()
        {
            var totalSteps = _profileSteps.Count();
            if (totalSteps <= 0)
            {
                Logger.Log("Profile is missing Profile Steps.");
                return;
            }
            var incompleteSteps = _profileSteps.Count(s => !s.IsCompleted);
            var completedSteps = _profileSteps.Count(s => s.IsCompleted);
            if (CurrentStep == null)
            {
                CurrentStep = _profileSteps[0];
            }
            //if (!CurrentStep.IsCompleted)
            //{
            //    CurrentStepType = CurrentStep.StepType;
            //    return;
            //}
            if (CurrentStep.IsCompleted)
            {
                if (completedSteps == totalSteps)
                {
                    Logger.Log("Profile is Done");
                    // Exit the dungeon / Unload profile etc...
                    return;
                }

                CurrentStep = _profileSteps.Find(step => !step.IsCompleted);
            }
        }
    }
}
