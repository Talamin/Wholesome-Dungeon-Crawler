using robotManager.Helpful;
using System.Collections.Generic;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    internal class RepositionInfo
    {
        public List<DangerZone> DangerZones { get; private set; }
        public ForcedSafeZone ForcedSafeZone { get; private set; }
        public DangerZone CurrentDangerZone { get; private set; }
        public bool InSafeZone { get; private set; }

        public RepositionInfo(
            List<DangerZone> dangerZones, 
            ForcedSafeZone forcedSafeZone, 
            DangerZone currentDangerZone,
            bool inSafeZone)
        {
            DangerZones = dangerZones;
            ForcedSafeZone = forcedSafeZone;
            CurrentDangerZone = currentDangerZone;
            InSafeZone = inSafeZone;
        }
    }
}
