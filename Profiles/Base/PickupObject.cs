using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class PickupObject : Step
    {
        private readonly int _objectId;
        private readonly uint _itemId;
        private readonly Vector3 _expectedPosition;
        private readonly bool _findClosest;
        private readonly bool _strictPosition;
        private readonly float _interactDistance;

        private readonly IMoveHelper _movehelper;

        public PickupObject(IMoveHelper imovehelper)
        {
            _movehelper = imovehelper;
        }

        public PickupObject(uint itemId,
            int objectId,
            Vector3 expectedPosition,
            string stepName = "PickupObject",
            bool findClosest = false,
            bool strictPosition = false,
            float interactDistance = 4f) : base(stepName)
        {
            _objectId = objectId;
            _expectedPosition = expectedPosition;
            _findClosest = findClosest;
            _strictPosition = strictPosition;
            _interactDistance = interactDistance;
            _itemId = itemId;
        }

        public override bool Pulse()
        {
            // Closest object from me or from its supposed position?
            Vector3 referencePosition = _strictPosition ? _expectedPosition : ObjectManager.Me.Position;

            WoWGameObject foundObject = _findClosest || _strictPosition
                ? FindClosestObject(gameObject => gameObject.Entry == _objectId, referencePosition)
                : ObjectManager.GetObjectWoWGameObject().FirstOrDefault(o => o.Entry == _objectId);

            // Move close to expected object position
            if (!_movehelper.IsMovementThreadRunning && ObjectManager.Me.PositionWithoutType.DistanceTo(_expectedPosition) > 10)
            {
                Logger.Log($"Moving to object {_objectId} at {_expectedPosition}");
                _movehelper.StartGoToThread(_expectedPosition);
                return IsCompleted = false;
            }

            // Check if we have object in bag
            if (ItemsManager.GetItemCountById(_itemId) > 0)
            {
                Logger.Log($"Picking up item {_itemId} complete");
                return IsCompleted = true;
            }

            // Is it present?
            if (foundObject == null)
            {
                Logger.Log($"Expected interactive object {_objectId} but it's absent");
                Thread.Sleep(2000);
                return IsCompleted = false;
            }

            // Move to real object position
            if ((!_movehelper.IsMovementThreadRunning && ObjectManager.Me.PositionWithoutType.DistanceTo(foundObject.Position) > _interactDistance))
            {
                Logger.Log($"Interactive object found. Approaching {_objectId} at {foundObject.Position}");
                _movehelper.StartGoToThread(foundObject.Position);
                return IsCompleted = false;
            }

            // Interact with object
            if (!_movehelper.IsMovementThreadRunning)
            {
                Interact.InteractGameObject(foundObject.GetBaseAddress);
                Usefuls.WaitIsCasting();
            }

            return IsCompleted;
        }

        private  WoWGameObject FindClosestObject(Func<WoWGameObject, bool> predicate, Vector3 referencePosition = null)
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
