using robotManager.Helpful;
using System;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Helpers
{
    class TargetingHelper
    {
        public static IWoWUnit FindClosestUnit(Func<IWoWUnit, bool> predicate, Vector3 referencePosition, IWoWUnit[] list)
        {
            IWoWUnit foundUnit = null;
            var distanceToUnit = float.MaxValue;

            Vector3 position = referencePosition;

            foreach (IWoWUnit unit in list)
            {
                if (!predicate(unit)) continue;
                if (TraceLine.TraceLineGo(unit.PositionWithoutType)) continue;

                if (foundUnit == null)
                {
                    distanceToUnit = position.DistanceTo(unit.PositionWithoutType);
                    foundUnit = unit;
                }
                else
                {
                    float currentDistanceToUnit = position.DistanceTo(unit.PositionWithoutType);
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
