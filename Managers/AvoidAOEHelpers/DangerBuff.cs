using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;

namespace WholesomeDungeonCrawler.Managers
{
    public class DangerBuff : IAvoidableEvent
    {
        public int UnitId { get; private set; }
        public int SpellId { get; private set; }
        public string Name { get; private set; }
        public Shape Shape { get; private set; }
        public float Size { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }

        public DangerBuff(int unitId, int spellId, string name, Shape shape, float size, List<LFGRoles> affectedRoles)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Name = name;
        }

        public bool PositionInDanger(Vector3 position, DangerZone zone, int margin = 0)
        {
            return position.DistanceTo(zone.Position) < Size + margin;
        }
        /*
        public void Draw(DangerZone zone)
        {
            Radar3D.DrawCircle(zone.Position, zone.Radius, Color.Orange, true, 100);
        }
        */
        public override bool Equals(object obj)
        {
            return obj is DangerBuff buff &&
                   UnitId == buff.UnitId &&
                   SpellId == buff.SpellId &&
                   Shape == buff.Shape &&
                   Size == buff.Size &&
                   EqualityComparer<List<LFGRoles>>.Default.Equals(AffectedRoles, buff.AffectedRoles);
        }

        public override int GetHashCode()
        {
            int hashCode = 383168334;
            hashCode = hashCode * -1521134295 + UnitId.GetHashCode();
            hashCode = hashCode * -1521134295 + SpellId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Shape.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<LFGRoles>>.Default.GetHashCode(AffectedRoles);
            return hashCode;
        }
    }
}