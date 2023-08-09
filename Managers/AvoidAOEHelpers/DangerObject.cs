using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    public class DangerObject : IAvoidableEvent
    {
        public ulong Guid { get; private set; }
        public WoWObject WowObject { get; private set; }
        public string Name { get; private set; }
        public float Size { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }
        public DangerType Type { get; private set; }

        public DangerObject(WoWObject wowObject, float radius)
        {
            WowObject = wowObject;
            Guid = wowObject.Guid;
            Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
            Size = radius;
            Type = DangerType.GameObject;
        }

        public bool PositionInDanger(Vector3 playerPosition, DangerZone zone)
        {
            return playerPosition.DistanceTo(zone.Position) < Size;
        }

        public void Draw(Vector3 position, DangerZone zone, Color color, bool filled, int alpha)
        {
            Radar3D.DrawCircle(position, Size, color, filled, alpha);
        }

        public override bool Equals(object obj)
        {
            return obj is DangerObject @object &&
                   Guid == @object.Guid &&
                   EqualityComparer<WoWObject>.Default.Equals(WowObject, @object.WowObject) &&
                   Name == @object.Name &&
                   Size == @object.Size &&
                   EqualityComparer<List<LFGRoles>>.Default.Equals(AffectedRoles, @object.AffectedRoles) &&
                   Type == @object.Type;
        }

        public override int GetHashCode()
        {
            int hashCode = 2030790684;
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<WoWObject>.Default.GetHashCode(WowObject);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<LFGRoles>>.Default.GetHashCode(AffectedRoles);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }
    }
}