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
        public override string Name { get; }

        public InteractWithStep(InteractWithModel interactWithModel, IEntityCache entityCache)
        {
            _interactWithModel = interactWithModel;
            _entityCache = entityCache;
            Name = interactWithModel.Name;
        }
        public override void Run()
        {
            // Closest object from me or from its supposed position?
            Vector3 referencePosition = _interactWithModel.StrictPosition ? _interactWithModel.ExpectedPosition : _entityCache.Me.PositionWithoutType;

            WoWGameObject foundObject = _interactWithModel.FindClosest || _interactWithModel.StrictPosition
                ? FindClosestObject(gameObject => gameObject.Entry == _interactWithModel.ObjectId, referencePosition)
                : ObjectManager.GetObjectWoWGameObject().FirstOrDefault(o => o.Entry == _interactWithModel.ObjectId);


            // Move close to expected object position
            if (!MovementManager.InMovement && ObjectManager.Me.PositionWithoutType.DistanceTo(_interactWithModel.ExpectedPosition) > 20 + 4f)
            {
                Logger.Log($"Moving to object {_interactWithModel.ObjectId} at {_interactWithModel.ExpectedPosition}");
                GoToTask.ToPosition(_interactWithModel.ExpectedPosition, 3.5f, false, context => ObjectManager.Me.PositionWithoutType.DistanceTo(foundObject.Position) <= 20 + 4f);
                IsCompleted = false;
                return;
            }

            // Is it present?
            if (foundObject == null)
            {
                Logger.Log($"Expected interactive object {_interactWithModel.ObjectId} but it's absent");
                Thread.Sleep(2000);
                IsCompleted = false;
                return;
            }

            // Move to real object position
            if (!MovementManager.InMovement && ObjectManager.Me.PositionWithoutType.DistanceTo(foundObject.Position) > 4f)
            {
                Logger.Log($"Interactive object found. Approaching {_interactWithModel.ObjectId} at {foundObject.Position}");
                GoToTask.ToPosition(foundObject.Position, 3.5f, false, context => ObjectManager.Me.PositionWithoutType.DistanceTo(foundObject.Position) <= 4f);
                IsCompleted = false;
                return;
            }

            // Interact with object
            if (!MovementManager.InMovement)
            {
                Interact.InteractGameObject(foundObject.GetBaseAddress);
                Usefuls.WaitIsCasting();
            }

            if (!_interactWithModel.CompleteCondition.HasCompleteCondition)
            {
                Logger.Log($"Interaction with object {foundObject.Entry} is complete");
                IsCompleted = true;
                return;
            }
            else if (EvaluateCompleteCondition(_interactWithModel.CompleteCondition))
            {
                Logger.Log($"Interaction with object {foundObject.Entry} is complete");
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