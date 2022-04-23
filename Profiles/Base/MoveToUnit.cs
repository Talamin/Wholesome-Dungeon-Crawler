using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class MoveToUnit : Step
    {
        private readonly Vector3 _expectedPosition;
        private readonly bool _findClosest;
        private readonly bool _skipIfNotFound;
        private readonly int _unitId;
        private readonly bool _interactwithunit;
        private readonly int _gossip;

        public MoveToUnit(int unitId, Vector3 expectedPosition, string stepName = "MoveToUnit",
            bool findClosest = false, bool skipIfNotFound = false, bool interactWithUnit = false, int Gossip = 1) : base(stepName)
        {
            _expectedPosition = expectedPosition;
            _findClosest = findClosest;
            _skipIfNotFound = skipIfNotFound;
            _unitId = unitId;
            _interactwithunit = interactWithUnit;
            _gossip = Gossip;
        }

        public override bool Pulse()
        {
            WoWUnit foundUnit = _findClosest
                ? FindClosestUnit(unit => unit.Entry == _unitId)
                : ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _unitId);

            Vector3 myPosition = ObjectManager.Me.PositionWithoutType;

            if (foundUnit == null)
            {
                if (myPosition.DistanceTo(_expectedPosition) > 4)
                {
                    // Goto expected position
                    GoToTask.ToPosition(_expectedPosition);
                }
                else if (_skipIfNotFound)
                {
                    Logger.LogDebug($"[Step {Name}]: Skipping unit {_unitId} because he's not here.");
                    IsCompleted = true;
                    return true;
                }
            }
            else
            {
                Vector3 targetPosition = foundUnit.PositionWithoutType;
                float targetInteractDistance = foundUnit.InteractDistance;
                if (myPosition.DistanceTo(targetPosition) < targetInteractDistance)
                {
                    if (_interactwithunit)
                    {
                        Interact.InteractGameObject(foundUnit.GetBaseAddress);
                        Usefuls.SelectGossipOption(_gossip);
                    }
                    IsCompleted = true;
                    return true;
                }
                else
                {
                    GoToTask.ToPosition(targetPosition);
                }
            }

            return false;
        }

        private WoWUnit FindClosestUnit(Func<WoWUnit, bool> predicate, Vector3 referencePosition = null)
        { //this function calculates the flosest Unit
            //first clear ol foundUnit
            WoWUnit foundUnit = null;
            var distanceToUnit = float.MaxValue;
            //checks for a given reference position, if not there then use our position
            Vector3 position = referencePosition != null ? referencePosition : ObjectManager.Me.Position;
            //build a List of each Unit and their Distance
            foreach (WoWUnit unit in ObjectManager.GetObjectWoWUnit())
            {
                if (!predicate(unit)) continue;

                if (foundUnit == null)
                {
                    distanceToUnit = position.DistanceTo(unit.Position);
                    foundUnit = unit;
                }
                else
                {
                    //float currentDistanceToUnit = myPosition.DistanceTo(unit.PositionWithoutType);
                    //checks the Distance of the Unit to the given Position
                    float currentDistanceToUnit = WTPathFinder.CalculatePathTotalDistance(position, unit.PositionWithoutType);
                    if (currentDistanceToUnit < distanceToUnit)
                    {
                        foundUnit = unit;
                        distanceToUnit = currentDistanceToUnit;
                    }
                }
            }
            return foundUnit;
        }
    }
}
