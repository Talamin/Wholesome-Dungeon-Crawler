using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles
{
    internal class Profile : IProfile
    {
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private List<IStep> _profileSteps = new List<IStep>();
        private IStep _currentStep;
        public List<PathFinder.OffMeshConnection> OffMeshConnectionsList = new List<PathFinder.OffMeshConnection>();

        public int MapId { get; }
        public List<Vector3> DeathRunPathList { get; private set; } = new List<Vector3>();
        public Dictionary<IStep, List<Vector3>> DungeonPath { get; private set; } = new Dictionary<IStep, List<Vector3>>();
        public FactionType FactionType { get; private set; }

        public Profile(ProfileModel profileModel, IEntityCache entityCache, IPathManager pathManager, IPartyChatManager partyChatManager)
        {
            _entityCache = entityCache;
            _partyChatManager = partyChatManager;

            foreach (StepModel model in profileModel.StepModels)
            {
                switch (model)
                {
                    case MoveAlongPathModel _:
                        MoveAlongPathStep step = new MoveAlongPathStep((MoveAlongPathModel)model, entityCache, pathManager);
                        DungeonPath.Add(step, step.GetMoveAlongPath);
                        _profileSteps.Add(step);
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
                        /*
                    case PickupObjectModel _:
                        _profileSteps.Add(new PickupObjectStep((PickupObjectModel)model, entityCache));
                        break;
                        */
                    case FollowUnitModel _:
                        FollowUnitModel fuModel = model as FollowUnitModel;
                        _entityCache.AddNpcIdToDefend(fuModel.UnitId);
                        _profileSteps.Add(new FollowUnitStep(fuModel, entityCache));
                        break;
                    case DefendSpotModel _:
                        _profileSteps.Add(new DefendSpotStep((DefendSpotModel)model, entityCache));
                        break;
                    case RegroupModel _:
                        _profileSteps.Add(new RegroupStep((RegroupModel)model, entityCache, partyChatManager));
                        break;
                }
            }

            foreach (Vector3 point in profileModel.DeathRunPath)
            {
                DeathRunPathList.Add(point);
            }

            PathFinder.OffMeshConnections.AddRange(profileModel.OffMeshConnections);

            MapId = profileModel.MapId;
            FactionType = profileModel.Faction;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            _entityCache.ClearNpcListIdToDefend();
        }
        
        public void SetCurrentStep(IStep step)
        {
            if (step is RegroupStep)
            {
                _partyChatManager.SetRegroupStep((RegroupStep)step);
            }
            _currentStep = step;
        }
        public IStep CurrentStep => _currentStep;

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

                if (_profileSteps[i] is RegroupStep)
                {
                    RegroupStep regroupStep = (RegroupStep)_profileSteps[i];
                    stepDistances[i] = regroupStep.RegroupSpot.DistanceTo(_entityCache.Me.PositionWithoutType);
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
            SetCurrentStep(_profileSteps[resultIndex]);
        }

        public void AutoSetCurrentStep()
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
                SetCurrentStep(_profileSteps[0]);
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

                SetCurrentStep(_profileSteps.Find(step => !step.IsCompleted));
            }
        }

        public bool ProfileIsCompleted => _profileSteps.All(p => p.IsCompleted);
    }
}
