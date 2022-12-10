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
                IsCompleted = false;
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

                if (_interactWithModel.SkipIfNotFound && EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
                {
                    Logger.Log($"[{_interactWithModel.Name}] Couldn't find interactive object {string.Join(" or ", _objectIds)}, skipping step");
                    IsCompleted = true;
                    return;
                }
                else
                {
                    Thread.Sleep(1000);
                    Logger.Log($"[Step {_interactWithModel.Name}]: Couldn't find interactive object {string.Join(" or ", _objectIds)} and SkipIfNotFound is false. Waiting.");
                    return;
                }
            }

            float realDistanceToObject = _entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position);

            // We reached the object, stop and evaluate completion
            if (realDistanceToObject <= _interactDistance)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
                if (EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
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
                IsCompleted = false;
                return;
            }

            // Interact with object
            if (!MovementManager.InMovement)
            {
                Logger.Log($"[{_interactWithModel.Name}] Interacting with {foundObject.Name} ({foundObject.Entry})");
                Interact.InteractGameObject(foundObject.GetBaseAddress);
                Usefuls.WaitIsCasting();
                Thread.Sleep(1000);
                // Press yes in case it's a bind on pickup
                if (Lua.LuaDoString<bool>($"return StaticPopup1Button1 and StaticPopup1Button1:IsVisible();"))
                {
                    Lua.LuaDoString<bool>($"StaticPopup1Button1:Click();");
                }
            }

            if (EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
            {
                Logger.Log($"[{_interactWithModel.Name}] Interaction with {foundObject.Name} ({foundObject.Entry}) is complete");
                IsCompleted = true;
                return;
            }

            return;
        }

        private WoWGameObject FindClosestObject(Func<WoWGameObject, bool> predicate, Vector3 referencePosition = null)
        { //same like FindClosestUnit
            WoWGameObject foundObject = null;
            var distanceToObject = float.MaxValue;
            Vector3 position = referencePosition != null ? referencePosition : ObjectManager.Me.Position;

            foreach (WoWGameObject gameObject in ObjectManager.GetObjectWoWGameObject())
            {
                if (!predicate(gameObject)) continue;

                if (foundObject == null)
                {
                    distanceToObject = position.DistanceTo(gameObject.Position);
                    foundObject = gameObject;
                }
                else
                {
                    float currentDistanceToObject = position.DistanceTo(gameObject.Position);
                    //float currentDistanceToObject = CalculatePathTotalDistance(position, gameObject.Position);
                    if (currentDistanceToObject < distanceToObject)
                    {
                        foundObject = gameObject;
                        distanceToObject = currentDistanceToObject;
                    }
                }
            }
            return foundObject;
        }
    }
}