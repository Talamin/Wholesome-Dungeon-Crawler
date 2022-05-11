using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveToUnitStep : Step
    {
        private MoveToUnitModel _moveToUnitModel;
        private readonly IEntityCache _entityCache;

        public MoveToUnitStep(MoveToUnitModel moveToUnitModel, IEntityCache entityCache)
            {
                _moveToUnitModel = moveToUnitModel;
                _entityCache = entityCache;
            }

            public override void Run()
            {
            WoWUnit foundUnit = _moveToUnitModel.FindClosest
                ? FindClosestUnit(unit => unit.Entry == _moveToUnitModel.UnitId)
                : ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _moveToUnitModel.UnitId);

            Vector3 myPosition = _entityCache.Me.PositionWithoutType;

            if (foundUnit == null)
            {
                if (myPosition.DistanceTo(_moveToUnitModel.ExpectedPosition) > 4)
                {
                    // Goto expected position
                    GoToTask.ToPosition(_moveToUnitModel.ExpectedPosition);
                }
                else if (_moveToUnitModel.SkipIfNotFound)
                {
                    Logger.LogDebug($"[Step {_moveToUnitModel.Name}]: Skipping unit {_moveToUnitModel.UnitId} because he's not here.");
                    IsCompleted = true;
                    return;
                }
            }
            else
            {
                Vector3 targetPosition = foundUnit.PositionWithoutType;
                float targetInteractDistance = foundUnit.InteractDistance;
                if (myPosition.DistanceTo(targetPosition) < targetInteractDistance)
                {
                    if (_moveToUnitModel.Interactwithunit)
                    {
                        Interact.InteractGameObject(foundUnit.GetBaseAddress);
                        Usefuls.SelectGossipOption(_moveToUnitModel.Gossip);
                    }
                    IsCompleted = true;
                    return;
                }
                else
                {
                    GoToTask.ToPosition(targetPosition);
                }
            }
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
