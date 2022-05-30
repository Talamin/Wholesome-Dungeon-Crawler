using robotManager.Helpful;
using System;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class PickupObjectStep : Step
    {
        private PickupObjectModel _pickupObjectModel;
        private readonly IEntityCache _entityCache;

        public PickupObjectStep(PickupObjectModel pickupObjectModel, IEntityCache entityCache)
        {
            _pickupObjectModel = pickupObjectModel;
            _entityCache = entityCache;
        }
        public override void Run()
        {
            // Closest object from me or from its supposed position?
            Vector3 referencePosition = _pickupObjectModel.StrictPosition ? _pickupObjectModel.ExpectedPosition : ObjectManager.Me.Position;

            WoWGameObject foundObject = _pickupObjectModel.FindClosest || _pickupObjectModel.StrictPosition
                ? FindClosestObject(gameObject => gameObject.Entry == _pickupObjectModel.ObjectId, referencePosition)
                : ObjectManager.GetObjectWoWGameObject().FirstOrDefault(o => o.Entry == _pickupObjectModel.ObjectId);

            // Move close to expected object position
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_pickupObjectModel.ExpectedPosition) > 10)
            {
                Logger.Log($"Moving to object {_pickupObjectModel.ObjectId} at {_pickupObjectModel.ExpectedPosition}");
                GoToTask.ToPosition(_pickupObjectModel.ExpectedPosition);
                IsCompleted = false;
                return;
            }

            // Check if we have object in bag
            if (ItemsManager.GetItemCountById(_pickupObjectModel.ItemId) > 0)
            {
                Logger.Log($"Picking up item {_pickupObjectModel.ItemId} complete");
                if (!_pickupObjectModel.CompleteCondition.HasCompleteCondition)
                {
                    IsCompleted = true;
                    return;
                }
                else if (EvaluateCompleteCondition(_pickupObjectModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }

            // Is it present?
            if (foundObject == null)
            {
                Logger.Log($"Expected interactive object {_pickupObjectModel.ObjectId} but it's absent");
                Thread.Sleep(2000);
                IsCompleted = false;
                return;
            }

            // Move to real object position
            if (_entityCache.Me.PositionWithoutType.DistanceTo(foundObject.Position) > _pickupObjectModel.InteractDistance)
            {
                Logger.Log($"Interactive object found. Approaching {_pickupObjectModel.ObjectId} at {foundObject.Position}");
                GoToTask.ToPosition(foundObject.Position);
                IsCompleted = false;
                return;
            }

            // Interact with object
            if (!MovementManager.InMovement)
            {
                Interact.InteractGameObject(foundObject.GetBaseAddress);
                Usefuls.WaitIsCasting();
            }

            IsCompleted = false;
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
