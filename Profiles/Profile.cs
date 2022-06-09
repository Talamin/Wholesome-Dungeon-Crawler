using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Profiles
{
    public class Profile : IProfile
    {
        private IEntityCache _entityCache;
        private ITargetingManager _targetingManager;
        private List<IStep> _profileSteps = new List<IStep>();
        public List<PathFinder.OffMeshConnection> OffMeshConnectionsList = new List<PathFinder.OffMeshConnection>();

        public int MapId { get; }
        public List<Vector3> DeathRunPathList { get; private set; } = new List<Vector3>();
        public IStep CurrentStep { get; private set; }
        public Dictionary<IStep, List<Vector3>> DungeonPath { get; private set; } = new Dictionary<IStep, List<Vector3>>();

        public Profile(ProfileModel profileModel, IEntityCache entityCache, ITargetingManager targetingManager)
        {
            _entityCache = entityCache;
            _targetingManager = targetingManager;

            foreach (StepModel model in profileModel.StepModels)
            {
                switch (model)
                {
                    case MoveAlongPathModel _:
                        MoveAlongPathStep step = new MoveAlongPathStep((MoveAlongPathModel)model, entityCache);
                        _profileSteps.Add(step);
                        DungeonPath.Add(step, step.GetMoveAlongPath);
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
            foreach (Vector3 point in profileModel.DeathRunPath)
            {
                DeathRunPathList.Add(point);
            }
            foreach(FollowUnitModel model in profileModel.StepModels)
            {
                targetingManager._npcsToDefendID.Add(model.UnitId);
            }

            PathFinder.OffMeshConnections.AddRange(profileModel.OffMeshConnections);

            MapId = profileModel.MapId;
        }

        public void Dispose()
        {
        }

        // A method to set the closest movealong step after a restart
        public void SetFirstLaunchStep()
        {
            int resultIndex = 0;
            int totalSteps = _profileSteps.Count();
            if (totalSteps <= 0)
            {
                Logger.Log("Profile is missing Profile Steps.");
                return;
            }

            Dictionary<int, float> stepDistances = new Dictionary<int, float>(); // step index - distance
            for (int i = 0; i < totalSteps; i++)
            {
                if (_profileSteps[i] is MoveAlongPathStep)
                {
                    // get closest node from this path
                    MoveAlongPathStep moveAlongStep = (MoveAlongPathStep)_profileSteps[i];
                    Vector3 closestNodeFromThisPath = moveAlongStep.GetMoveAlongPath
                        .OrderBy(node => node.DistanceTo(_entityCache.Me.PositionWithoutType))
                        .First();
                    stepDistances[i] = closestNodeFromThisPath.DistanceTo(_entityCache.Me.PositionWithoutType);
                }
            }

            // we order the dictionary by distance
            Dictionary<int, float> orderedStepDistances = stepDistances
                .OrderBy(entry => entry.Value)
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            resultIndex = orderedStepDistances.ElementAt(0).Key;
            float radiusToCheck = orderedStepDistances.ElementAt(0).Value + 20;
            foreach (KeyValuePair<int, float> pair in orderedStepDistances)
            {
                if (pair.Key < resultIndex && pair.Value < radiusToCheck)
                {
                    resultIndex = pair.Key;
                }
            }

            // mark previous steps as completed
            for (int i = 0; i < totalSteps; i++)
            {
                if (i < resultIndex)
                {
                    Logger.Log($"Marked {_profileSteps[i].Name} as completed");
                    _profileSteps[i].MarkAsCompleted();
                }
                else
                {
                    break;
                }
            }

            Logger.Log($"Setting {_profileSteps[resultIndex].Name} as current");
            CurrentStep = _profileSteps[resultIndex];
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

            if (CurrentStep.IsCompleted)
            {
                Logger.Log($"Completed Current Step: {CurrentStep.Name}");
                if (completedSteps == totalSteps)
                {
                    Logger.Log("Profile is Done");
                    // Exit the dungeon / Unload profile etc...
                    return;
                }

                CurrentStep = _profileSteps.Find(step => !step.IsCompleted);
            }
        }

        public bool ProfileIsCompleted => _profileSteps.All(p => p.IsCompleted);
    }
}
