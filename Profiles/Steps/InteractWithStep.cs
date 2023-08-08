using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class InteractWithStep : Step
    {
        private InteractWithModel _interactWithModel;
        private readonly IEntityCache _entityCache;
        private float _interactDistance;
        private Vector3 _expectedPosition;
        private List<int> _objectIds = new List<int>();

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }

        public InteractWithStep(InteractWithModel interactWithModel, IEntityCache entityCache) : base(interactWithModel.CompleteCondition)
        {
            _interactWithModel = interactWithModel;
            _entityCache = entityCache;
            Name = interactWithModel.Name;
            StepFaction = interactWithModel.StepFaction;
            StepRole = interactWithModel.StepRole;
            _interactDistance = interactWithModel.InteractDistance < 3.5f ? 3.5f : interactWithModel.InteractDistance;
            _expectedPosition = interactWithModel.ExpectedPosition;
            string[] objectsIds = interactWithModel.ObjectId.Split(';');
            foreach (string objectId in objectsIds)
            {
                _objectIds.Add(int.Parse(objectId));
            }
            PreEvaluationPass = EvaluateFactionCompletion() && EvaluateRoleCompletion();
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

            // Keep checking while we cast
            if (ObjectManager.Me.IsCast
                && _interactWithModel.CompleteCondition.ConditionType != CompleteConditionType.None
                && !EvaluateCompleteCondition())
            {
                Thread.Sleep(500);
                return;
            }

            // Closest object from me or from its expected position?
            Vector3 referencePosition = _expectedPosition != null ? _expectedPosition : _entityCache.Me.PositionWT;
            WoWGameObject foundObject = ObjectManager.GetObjectWoWGameObject()
                .Where(go => _objectIds.Contains(go.Entry))
                .OrderBy(go => go.Position.DistanceTo(referencePosition))
                .FirstOrDefault();

            // Move close if we have an expected object position
            if (_expectedPosition != null
                && !MovementManager.InMovement
                && _entityCache.Me.PositionWT.DistanceTo(_expectedPosition) > _interactDistance + 5f)
            {
                Logger.LogOnce($"[{_interactWithModel.Name}] Moving to object's expected position {_expectedPosition} with interact distance {_interactDistance}");
                List<Vector3> pathToObject = PathFinder.FindPath(_expectedPosition);
                MovementManager.Go(pathToObject);
                return;
            }

            // Object is absent
            if (foundObject == null)
            {
                // Suggest object
                if (_expectedPosition != null)
                {
                    WoWGameObject objectAtLocation = ObjectManager.GetObjectWoWGameObject()
                        .Where(obj => obj.Position == _expectedPosition)
                        .FirstOrDefault();
                    if (objectAtLocation != null)
                    {
                        Logger.LogError($"Couldn't find object with entry {string.Join(" or ", _objectIds)}, but found {objectAtLocation.Name} ({objectAtLocation.Entry}). If this is the object you're looking for, please add the correct object entry to the list in your profile.");
                    }
                }

                if (EvaluateCompleteCondition())
                {
                    if (_interactWithModel.SkipIfNotFound)
                    {
                        Logger.Log($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)}. Condition is a PASS and skipping is allowed. Marking step as completed.");
                        MarkAsCompleted();
                    }
                    else
                    {
                        Logger.LogOnce($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)}. Condition is a PASS but skipping is not allowed. Waiting.");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    if (_interactWithModel.SkipIfNotFound)
                    {
                        Logger.Log($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)} and condition is a FAIL but skipping is allowed. Marking step as completed.");
                        MarkAsCompleted();
                    }
                    else
                    {
                        Logger.LogOnce($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)} and condition is a FAIL. Waiting.");
                        Thread.Sleep(1000);
                    }
                }

                return;
            }

            float realDistanceToObject = _entityCache.Me.PositionWT.DistanceTo(foundObject.Position);

            // We reached the object, stop and evaluate completion
            if (realDistanceToObject <= _interactDistance)
            {
                MovementManager.StopMove();
                if (_interactWithModel.CompleteCondition.ConditionType != CompleteConditionType.None
                    && EvaluateCompleteCondition())
                {
                    Logger.Log($"[{_interactWithModel.Name}] Interaction with object {foundObject.Name} ({foundObject.Entry}) is complete");
                    MarkAsCompleted();
                    return;
                }
            }
            else
            {
                // Move to real object position
                if (!MovementManager.InMovement)
                {
                    Logger.Log($"[{_interactWithModel.Name}] Object found. Moving to {foundObject.Name} ({foundObject.Entry}) - ({realDistanceToObject}/{_interactDistance})");
                    List<Vector3> pathToObject = PathFinder.FindPath(foundObject.Position);
                    MovementManager.Go(pathToObject);
                    return;
                }
            }


            // Interact with object
            if (!MovementManager.InMovement && !ObjectManager.Me.IsCast)
            {
                // Press yes in case it's a bind on pickup
                if (Lua.LuaDoString<bool>($"return StaticPopup1Button1 and StaticPopup1Button1:IsVisible();"))
                {
                    Lua.LuaDoString<bool>($"StaticPopup1Button1:Click();");
                    Thread.Sleep(500);
                }
                else
                {
                    Logger.LogOnce($"[{_interactWithModel.Name}] Interacting with {foundObject.Name} ({foundObject.Entry})");
                    Interact.InteractGameObject(foundObject.GetBaseAddress);
                    Thread.Sleep(500);
                }

                if (_interactWithModel.CompleteCondition.ConditionType == CompleteConditionType.None
                    && !ObjectManager.Me.IsCast)
                {
                    Logger.Log($"[{_interactWithModel.Name}] Interaction with object {foundObject.Name} ({foundObject.Entry}) is complete (no conditions)");
                    MarkAsCompleted();
                    return;
                }
            }
        }
    }
}