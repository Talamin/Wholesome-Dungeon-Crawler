using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
//using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles
{
    public class Profile : IProfile
    {
        public List<IStep> _profileSteps = new List<IStep>();

        public IStep CurrentStep { get; private set; }

        public List<Vector3> DeathRunPathList = new List<Vector3>();
        public List<PathFinder.OffMeshConnection> OffMeshConnectionsList = new List<PathFinder.OffMeshConnection>();

        public Profile(ProfileModel profileModel, IEntityCache entityCache)
        {
            foreach (StepModel model in profileModel.StepModels)
            {
                switch (model)
                {
                    case MoveAlongPathModel _:
                        _profileSteps.Add(new MoveAlongPathStep((MoveAlongPathModel)model, entityCache));
                        break;
                    case GoToModel _:
                        _profileSteps.Add(new GoToStep((GoToModel)model, entityCache));
                        break;
                    case ExecuteModel _:
                        _profileSteps.Add(new ExecuteStep((ExecuteModel)model, entityCache));
                        break;
                    case InteractWithModel _:
                        _profileSteps.Add(new InteractWithStep((InteractWithModel)model, entityCache));
                        break;
                    case MoveToUnitModel _:
                        _profileSteps.Add(new MoveToUnitStep((MoveToUnitModel)model, entityCache));
                        break;
                    case PickupObjectModel _:
                        _profileSteps.Add(new PickupObjectStep((PickupObjectModel)model, entityCache));
                        break;
                    case FollowUnitModel _:
                        _profileSteps.Add(new FollowUnitStep((FollowUnitModel)model, entityCache));
                        break;
                    case DefendSpotModel _:
                        _profileSteps.Add(new DefendSpotStep((DefendSpotModel)model, entityCache));
                        break;
                }
                //elseif...
            }
            foreach(Vector3 point in profileModel.DeathRunPath)
            {
                DeathRunPathList.Add(point);
            }

            PathFinder.OffMeshConnections.AddRange(profileModel.OffMeshConnections);
        }

        public void Dispose()
        {
        }

        public void SetCurrentStep()
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
