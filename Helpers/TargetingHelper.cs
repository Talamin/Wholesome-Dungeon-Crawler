using System.ComponentModel;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Helpers
{
    class TargetingHelper
    {
        /*
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
        */

        public static void SwitchTargetAndFight(IWoWUnit unit, CancelEventArgs canceable)
        {
            canceable.Cancel = true;
            ObjectManager.Me.Target = unit.Guid;
            Fight.StartFight(unit.Guid, false);
        }

        public enum TargetPriority
        {
            High,
            Normal,
            Low
        }

        public static TargetPriority GetTargetPriority(IWoWUnit unit)
        {
            switch (unit.UnitID)
            {
                case 8996: return TargetPriority.Low; // RFC - Voidwalker minion
                case 598: return TargetPriority.High; // Deadmines - Defias Miner
                case 2520: return TargetPriority.Low; // Deadmines - Remote-Controlled Golem
                default: return TargetPriority.Normal;
            }
        }
    }
}
