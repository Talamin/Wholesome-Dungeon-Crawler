using robotManager.Helpful;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    internal class DangerZone
    {
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public ulong Guid { get; private set; }
        public string Name { get; private set; }
        public WoWObjectType ObjectType { get; private set; }

        public DangerZone(WoWObject wowObject, float radius)
        {
            Position = wowObject.Position;
            Guid = wowObject.Guid;
            Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
            ObjectType = wowObject.Type;
            Radius = radius;
        }

        public bool PositionInDangerZone(Vector3 position, int margin = 0)
        {
            return Position.DistanceTo(position) < Radius + margin;
        }
    }
}
