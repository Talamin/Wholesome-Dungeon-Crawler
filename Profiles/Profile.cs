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
using static wManager.Wow.Helpers.PathFinder;

namespace WholesomeDungeonCrawler.Profiles
{
    internal class Profile : IProfile
    {
        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private readonly IProfileManager _profileManager;
        private List<IStep> _profileSteps = new List<IStep>();
        private IStep _currentStep;
        public List<PathFinder.OffMeshConnection> OffMeshConnectionsList = new List<PathFinder.OffMeshConnection>();

        public int MapId { get; }
        public List<Vector3> DeathRunPath { get; private set; } = new List<Vector3>();
        public Dictionary<IStep, List<Vector3>> DungeonPath { get; private set; } = new Dictionary<IStep, List<Vector3>>();
        public List<Vector3> AllMoveAlongNodes { get; private set; } = new List<Vector3>();
        public FactionType FactionType { get; private set; }
        public string FileName { get; private set; }
        public int GetCurrentStepIndex => _profileSteps.IndexOf(_currentStep);
        public bool ProfileIsCompleted => _profileSteps.All(p => p.IsCompleted);
        public List<IStep> GetAllSteps => _profileSteps;
        public IStep CurrentStep => _currentStep;
        public DungeonModel DungeonModel { get; private set; }

        public Profile(ProfileModel profileModel,
                IEntityCache entityCache,
                IPathManager pathManager,
                IPartyChatManager partyChatManager,
                IProfileManager profileManager,
                string fileName)
        {
            _entityCache = entityCache;
            _partyChatManager = partyChatManager;
            _profileManager = profileManager;
            FileName = fileName;
            DungeonModel = profileModel.DungeonModel;

            for (int i = 0; i < profileModel.StepModels.Count; i++)
            {
                StepModel model = profileModel.StepModels[i];
                switch (model)
                {
                    case MoveAlongPathModel _:
                        MoveAlongPathStep maStep = new MoveAlongPathStep((MoveAlongPathModel)model, entityCache, pathManager, i);
                        DungeonPath.Add(maStep, maStep.GetMoveAlongPath);
                        AllMoveAlongNodes.AddRange(maStep.GetMoveAlongPath);
                        _profileSteps.Add(maStep);
                        break;
                    case InteractWithModel _:
                        InteractWithModel interactWithModel = model as InteractWithModel;
                        AllMoveAlongNodes.Add(interactWithModel.ExpectedPosition);
                        _profileSteps.Add(new InteractWithStep(interactWithModel, entityCache));
                        break;
                    case TalkToUnitModel _:
                        TalkToUnitModel talkToUnitModel = model as TalkToUnitModel;
                        AllMoveAlongNodes.Add(talkToUnitModel.ExpectedPosition);
                        _profileSteps.Add(new TalkToUnitStep(talkToUnitModel, entityCache));
                        break;
                    case FollowUnitModel _:
                        FollowUnitModel fuModel = model as FollowUnitModel;
                        AllMoveAlongNodes.Add(fuModel.ExpectedStartPosition);
                        AllMoveAlongNodes.Add(fuModel.ExpectedEndPosition);
                        _entityCache.AddNpcIdToDefend(fuModel.UnitId);
                        _profileSteps.Add(new FollowUnitStep(fuModel, entityCache));
                        break;
                    case DefendSpotModel _:
                        DefendSpotModel defendSpotModel = model as DefendSpotModel;
                        AllMoveAlongNodes.Add(defendSpotModel.DefendPosition);
                        _profileSteps.Add(new DefendSpotStep(defendSpotModel, entityCache));
                        break;
                    case RegroupModel _:
                        RegroupModel regroupModel = model as RegroupModel;
                        AllMoveAlongNodes.Add(regroupModel.RegroupSpot);
                        _profileSteps.Add(new RegroupStep((RegroupModel)model, entityCache, partyChatManager));
                        break;
                    case JumpToStepModel _:
                        JumpToStepModel jumpToStepModel = model as JumpToStepModel;
                        _profileSteps.Add(new JumpToStepStep((JumpToStepModel)model, this));
                        break;
                    case LeaveDungeonModel _:
                        LeaveDungeonModel leaveDungeonModel = model as LeaveDungeonModel;
                        _profileSteps.Add(new LeaveDungeonStep((LeaveDungeonModel)model, entityCache, partyChatManager, profileManager));
                        break;
                    case PullToSafeSpotModel _:
                        PullToSafeSpotModel pullToSafeSpotModel = model as PullToSafeSpotModel;
                        _profileSteps.Add(new PullToSafeSpotStep((PullToSafeSpotModel)model, entityCache));
                        break;
                }
            }

            // Add default leave dungeon step at the end if it doesn't exist
            if (!(_profileSteps.Last() is LeaveDungeonStep))
            {
                _profileSteps.Add(new LeaveDungeonStep(new LeaveDungeonModel() { Name = "Leave dungeon (default)" }, entityCache, partyChatManager, profileManager));
            }

            AllMoveAlongNodes.RemoveAll(node => node == null);

            foreach (Vector3 point in profileModel.DeathRunPath)
            {
                DeathRunPath.Add(point);
            }

            // Clear all dungeon crawler offmesh connections
            int removed =OffMeshConnections.MeshConnection.RemoveAll(con => con.Name.StartsWith("WDCOMS - "));
            if (removed > 0)
            {
                Logger.Log($"Cleared {removed} crawler offmesh connections");
            }

            // Add profile offmesh connections
            foreach (OffMeshConnection omConnection in profileModel.OffMeshConnections)
            {
                if (omConnection == null || omConnection.Path.Count <= 0) continue;
                omConnection.Name = $"WDCOMS - {FileName} - {omConnection.Name}";
                if (!OffMeshConnections.MeshConnection.Exists(con => con.Name == omConnection.Name))
                {
                    OffMeshConnections.Add(omConnection);
                    Logger.Log($"Addded offmesh connection [{omConnection.Name}]");
                }
            }

            OffMeshConnections.AddRange(profileModel.OffMeshConnections);

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
            Logger.Log($"Setting current step to {step.Name}");
            _currentStep = step;
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
                    _profileSteps[i].MarkAsCompleted();
                    continue;
                }
                break;
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

        public bool JumpToStep(string jumpStepName, string stepToJumpTo)
        {
            List<IStep> correspondingSteps = _profileSteps
                .Where(step => step.Name == stepToJumpTo)
                .ToList();

            if (correspondingSteps.Count == 0)
            {
                Logger.LogOnce($"[{jumpStepName}] There is no step in your profile with the name [{stepToJumpTo}]", true);
                return false;
            }

            if (correspondingSteps.Count > 1)
            {
                Logger.LogOnce($"[{jumpStepName}] There are multiple steps in your profile with the name [{stepToJumpTo}]. Step name must be unique.", true);
                return false;
            }

            IStep stepToGo = correspondingSteps[0];
            int currentStepIndex = _profileSteps.IndexOf(CurrentStep);
            int stepToGoIndex = _profileSteps.IndexOf(stepToGo);

            if (stepToGoIndex < currentStepIndex)
            {
                Logger.LogOnce($"[{jumpStepName}] You're trying to jump to a previous step [{stepToJumpTo}]. You can only jump to a next step.", true);
                return false;
            }

            // mark previous steps as completed
            for (int i = 0; i < _profileSteps.Count; i++)
            {
                if (i < stepToGoIndex)
                {
                    _profileSteps[i].MarkAsCompleted();
                    continue;
                }
                break;
            }

            Logger.Log($"Jumped to {stepToGo.Name}");
            SetCurrentStep(stepToGo);

            return true;
        }
    }
}
