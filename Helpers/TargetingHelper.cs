using System.ComponentModel;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Helpers
{
    class TargetingHelper
    {
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
