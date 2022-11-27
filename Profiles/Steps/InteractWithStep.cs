using robotManager.Helpful;
using System;
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
        private int _interactDistance;
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
            _interactDistance = interactWithModel.InteractDistance;
            _expectedPosition = interactWithModel.ExpectedPosition;
            _objectId = interactWithModel.ObjectId;
        }
        public override void Run()
        {
            // Closest object from me or from its supposed position?
            Vector3 referencePosition = _interactWithModel.StrictPosition ? _expectedPosition : _entityCache.Me.PositionWithoutType;

            WoWGameObject foundObject = _interactWithModel.FindClosest || _interactWithModel.StrictPosition
                ? FindClosestObject(gameObject => gameObject.Entry == _objectId, referencePosition)
                : ObjectManager.GetObjectWoWGameObject().FirstOrDefault(o => o.Entry == _objectId);

            // We reached the object, stop and wait
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_expectedPosition) <= _interactDistance
                || foundObject != null && _entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position) <= _interactDistance + 0.5f)
            {
                MovementManager.StopMove();
            }

            // Move close to expected object position
            if (!MovementManager.InMovement && _entityCache.Me.PositionWithoutType.DistanceTo(_expectedPosition) > 20f)
            {
                Logger.Log($"[{_interactWithModel.Name}] Moving closer to object {_objectId} at {_expectedPosition} with interact distance {_interactDistance}");
                GoToTask.ToPosition(_expectedPosition, _interactDistance);
                IsCompleted = false;
                return;
            }

            // Is it present?
            if (foundObject == null)
            {
                Logger.Log($"[{_interactWithModel.Name}] Expected interactive object {_objectId} but it's absent");
                Thread.Sleep(2000);
                IsCompleted = false;
                return;
            }

            // Move to real object position
            if (!MovementManager.InMovement && _entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position) > _interactDistance + 0.5f)
            {
                Logger.Log($"[{_interactWithModel.Name}] Interactive object found. Approaching {_objectId} with interact distance {_interactDistance}");
                GoToTask.ToPosition(_expectedPosition, _interactDistance);
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