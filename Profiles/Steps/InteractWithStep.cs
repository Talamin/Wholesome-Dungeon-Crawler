using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class InteractWithStep : Step
    {
        private InteractWithModel _interactWithModel;
        private readonly IEntityCache _entityCache;
        private float _interactDistance;
        private Vector3 _expectedPosition;
        public override string Name { get; }
        public override int Order { get; }
        private List<int> _objectIds = new List<int>();

        public InteractWithStep(InteractWithModel interactWithModel, IEntityCache entityCache)
        {
            _interactWithModel = interactWithModel;
            _entityCache = entityCache;
            Name = interactWithModel.Name;
            Order = interactWithModel.Order;
            _interactDistance = interactWithModel.InteractDistance < 3.5f ? 3.5f : interactWithModel.InteractDistance;
            _expectedPosition = interactWithModel.ExpectedPosition;
            string[] objectsIds = interactWithModel.ObjectId.Split(';');
            foreach (string objectId in objectsIds)
            {
                _objectIds.Add(int.Parse(objectId));
            }
        }
        public override void Run()
        {
            // Keep checking while we cast
            if (ObjectManager.Me.IsCast 
                && _interactWithModel.CompleteCondition.ConditionType != CompleteConditionType.None
                && !EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
            {
                Thread.Sleep(500);
                return;
            }

            if (ObjectManager.Me.IsCast
                && _interactWithModel.CompleteCondition.ConditionType == CompleteConditionType.None)
            {
                Thread.Sleep(500);
                return;
            }

            // Closest object from me or from its expected position?
            Vector3 referencePosition = _expectedPosition != null ? _expectedPosition : _entityCache.Me.PositionWithoutType;
            WoWGameObject foundObject = ObjectManager.GetObjectWoWGameObject()
                .Where(go => _objectIds.Contains(go.Entry))
                .OrderBy(go => go.Position.DistanceTo(referencePosition))
                .FirstOrDefault();

            // Move close if we have an expected object position
            if (!MovementManager.InMovement
                && _expectedPosition != null
                && _entityCache.Me.PositionWithoutType.DistanceTo(_expectedPosition) > _interactDistance + 5f)
            {
                Logger.Log($"[{_interactWithModel.Name}] Moving to object's expected position {_expectedPosition} with interact distance {_interactDistance}");
                GoToTask.ToPosition(_expectedPosition, _interactDistance);
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

                if (EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
                {
                    if (_interactWithModel.SkipIfNotFound)
                    {
                        Logger.Log($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)}. Condition is a PASS and skipping is allowed. Marking step as completed.");
                        IsCompleted = true;
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
                        IsCompleted = true;
                    }
                    else
                    {
                        Logger.LogOnce($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)} and condition is a FAIL. Waiting.");
                        Thread.Sleep(1000);
                    }
                }

                return;
            }

            float realDistanceToObject = _entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position);

            // We reached the object, stop and evaluate completion
            if (realDistanceToObject <= _interactDistance)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
                if (_interactWithModel.CompleteCondition.ConditionType != CompleteConditionType.None
                    && EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
                {
                    Logger.Log($"[{_interactWithModel.Name}] Interaction with object {foundObject.Name} ({foundObject.Entry}) is complete");
                    IsCompleted = true;
                    return;
                }
            }

            // Move to real object position
            if (!MovementManager.InMovement
                && !MovementManager.InMoveTo
                && realDistanceToObject > _interactDistance)
            {
                if (realDistanceToObject > _interactDistance + 5)
                {
                    Logger.Log($"[{_interactWithModel.Name}] Object found. Long move to {foundObject.Name} ({foundObject.Entry}) - ({realDistanceToObject}/{_interactDistance})");
                    GoToTask.ToPosition(foundObject.Position, _interactDistance);
                }
                else
                {
                    Logger.Log($"[{_interactWithModel.Name}] Object found. Short move to {foundObject.Name} ({foundObject.Entry}) - ({realDistanceToObject}/{_interactDistance})");
                    MovementManager.MoveTo(foundObject.Position);
                }

                return;
            }

            // Interact with object
            if (!MovementManager.InMovement)
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
                    if (_interactWithModel.CompleteCondition.ConditionType == CompleteConditionType.None)
                    {
                        Logger.Log($"[{_interactWithModel.Name}] Interaction with object {foundObject.Name} ({foundObject.Entry}) is complete (no conditions)");
                        IsCompleted = true;
                        return;
                    }
                }
            }
        }
    }
}