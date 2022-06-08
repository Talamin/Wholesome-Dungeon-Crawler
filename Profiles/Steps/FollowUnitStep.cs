using robotManager.Helpful;
using System;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class FollowUnitStep : Step
    {
        private FollowUnitModel _followUnitModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }
        public override int Order { get; }

        public FollowUnitStep(FollowUnitModel followUnitModel, IEntityCache entityCache)
        {
            _followUnitModel = followUnitModel;
            _entityCache = entityCache;
            Name = followUnitModel.Name;
            Order = followUnitModel.Order;
        }

        public override void Run()
        {
            {
                WoWUnit foundUnit = _followUnitModel.FindClosest
                    ? FindClosestUnit(unit => unit.Entry == _followUnitModel.UnitId)
                    : ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _followUnitModel.UnitId);

                Vector3 myPosition = _entityCache.Me.PositionWithoutType;

                if (foundUnit == null)
                {
                    if (myPosition.DistanceTo(_followUnitModel.ExpectedEndPosition) >= 15)
                    {
                        // Goto expected position
                        GoToTask.ToPosition(_followUnitModel.ExpectedStartPosition, 3.5f, false, context => IsCompleted);
                    }
                    else if (_followUnitModel.SkipIfNotFound || myPosition.DistanceTo(_followUnitModel.ExpectedEndPosition) < 15)
                    {
                        if (!_followUnitModel.CompleteCondition.HasCompleteCondition)
                        {
                            Logger.LogDebug($"[Step {_followUnitModel.Name}]: Skipping unit {_followUnitModel.UnitId} because he's not here.");
                            IsCompleted = true;
                            return;
                        }
                        else if (EvaluateCompleteCondition(_followUnitModel.CompleteCondition))
                        {
                            IsCompleted = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (myPosition.DistanceTo(_followUnitModel.ExpectedEndPosition) < 15)
                    {
                        if (!_followUnitModel.CompleteCondition.HasCompleteCondition)
                        {
                            Logger.LogDebug($"[Step {_followUnitModel.Name}]: Skipping Step with {_followUnitModel.UnitId} because we reached our Enddestination.");
                            IsCompleted = true;
                            return;
                        }
                        else if (EvaluateCompleteCondition(_followUnitModel.CompleteCondition))
                        {
                            IsCompleted = true;
                            return;
                        };
                    }
                    Vector3 targetPosition = foundUnit.PositionWithoutType;
                    float followDistance = 25;

                    foreach(IWoWUnit unit in _entityCache.EnemyUnitsList)
                    { 
                        if(unit.TargetGuid == foundUnit.Guid)
                        {
                            Logger.Log("Defending Follow Unit");
                            ObjectManager.Me.Target = unit.Guid;
                            Fight.StartFight(unit.Guid, false);
                        }
                    }

                    if (!MovementManager.InMovement ||
                        _entityCache.Me.PositionWithoutType.DistanceTo(targetPosition) > followDistance)
                    {
                        GoToTask.ToPosition(targetPosition, 2.5f, false, context => _entityCache.Me.PositionWithoutType.DistanceTo(targetPosition) <= 5);
                    }
                }

                IsCompleted = false;


            }
        }
        private WoWUnit FindClosestUnit(Func<WoWUnit, bool> predicate, Vector3 referencePosition = null)
        { //this function calculates the flosest Unit
            //first clear ol foundUnit
            WoWUnit foundUnit = null;
            var distanceToUnit = float.MaxValue;
            //checks for a given reference position, if not there then use our position
            Vector3 position = referencePosition != null ? referencePosition : _entityCache.Me.PositionWithoutType;
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
