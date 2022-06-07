using robotManager.Helpful;
using System;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

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
                //if (TraceLine.TraceLineGo(unit.PositionWithoutType)) continue;

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

        public static bool IHaveLineOfSightOn(WoWObject wowObject)
        {
            Vector3 myPos = ObjectManager.Me.Position;
            Vector3 objectPos = (wowObject is WoWUnit) ? new Vector3(wowObject.Position.X, wowObject.Position.Y, wowObject.Position.Z + 2) : wowObject.Position;
            return !TraceLine.TraceLineGo(new Vector3(myPos.X, myPos.Y, myPos.Z + 2),
                objectPos,
                CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
        }
    }
}
