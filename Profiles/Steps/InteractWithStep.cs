using robotManager.Helpful;
using System;
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
        private int _objectId;
        private Vector3 _expectedPosition;
        public override string Name { get; }
        public override int Order { get; }

        public InteractWithStep(InteractWithModel interactWithModel, IEntityCache entityCache)
        {
            _interactWithModel = interactWithModel;
            _entityCache = entityCache;
            Name = interactWithModel.Name;
            Order = interactWithModel.Order;
            _interactDistance = interactWithModel.InteractDistance < 3.5f ? 3.5f : interactWithModel.InteractDistance;
            _expectedPosition = interactWithModel.ExpectedPosition;
            _objectId = interactWithModel.ObjectId;
        }
        public override void Run()
        {
            // Closest object from me or from its expected position?
            Vector3 referencePosition = _expectedPosition != null ? _expectedPosition : _entityCache.Me.PositionWithoutType;
            WoWGameObject foundObject = FindClosestObject(gameObject => gameObject.Entry == _objectId, referencePosition);

            // Move close if we have an expected object position
            if (!MovementManager.InMovement
                && _expectedPosition != null
                && _entityCache.Me.PositionWithoutType.DistanceTo(_expectedPosition) > _interactDistance + 5f)
            {
                Logger.Log($"[{_interactWithModel.Name}] Moving to object {_objectId} expected position {_expectedPosition} with interact distance {_interactDistance}");
                GoToTask.ToPosition(_expectedPosition, _interactDistance);
                IsCompleted = false;
                return;
            }

            // Object is absent
            if (foundObject == null)
            {
                Logger.Log($"[{_interactWithModel.Name}] Expected interactive object {_objectId} but it's absent, skipping step");
                IsCompleted = true;
                return;
            }

            float realDistanceToObject = _entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position);

            // We reached the object, stop and evaluate completion
            if (realDistanceToObject <= _interactDistance)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
                if (EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
                {
                    Logger.Log($"[{_interactWithModel.Name}] Interaction with object {_objectId} is complete");
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
                    Logger.Log($"[{_interactWithModel.Name}] Object found. Long move to {_objectId} ({realDistanceToObject}/{_interactDistance})");
                    GoToTask.ToPosition(foundObject.Position, _interactDistance);
                }
                else
                {
                    Logger.Log($"[{_interactWithModel.Name}] Object found. Short move to {_objectId} ({realDistanceToObject}/{_interactDistance})");
                    MovementManager.MoveTo(foundObject.Position);
                }
                IsCompleted = false;
                return;
            }

            // Interact with object
            if (!MovementManager.InMovement)
            {
                Logger.Log($"[{_interactWithModel.Name}] Interacting with {_objectId}");
                Interact.InteractGameObject(foundObject.GetBaseAddress);
                Usefuls.WaitIsCasting();
            }

            if (!_interactWithModel.CompleteCondition.HasCompleteCondition || EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
            {
                Logger.Log($"[{_interactWithModel.Name}] Interaction with object {foundObject.Entry} is complete");
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