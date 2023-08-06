using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    public class ForcedSafeZone
    {
        public int BossId { get; private set; }
        public Vector3 ZoneCenter { get; private set; }
        public int Radius { get; private set; }

        public ForcedSafeZone(int bossId, Vector3 zoneCenter, int radius)
        {
            BossId = bossId;
            ZoneCenter = zoneCenter;
            Radius = radius;
        }
        public bool PositionInSafeZone(Vector3 position, int margin = 0)
        {
            return ZoneCenter.DistanceTo(position) < Radius + margin;
        }
    }
}
