using System.Collections.Generic;
using System;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using robotManager.Helpful;
using System.Drawing.Printing;
using System.Drawing;
using wManager.Wow.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;

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
        public Func<bool> Condition { get; private set; }
        public bool IsConditionMet => Condition == null || Condition();

        public DangerBuff(int unitId, int spellId, String name, Shape shape, float size, List<LFGRoles> affectedRoles, Func<bool> condition = null)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Condition = condition;
            Name= name;
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
            return obj is DangerBuff buff &&
                   UnitId == buff.UnitId &&
                   SpellId == buff.SpellId &&
                   Shape == buff.Shape &&
                   Size == buff.Size &&
                   EqualityComparer<List<LFGRoles>>.Default.Equals(AffectedRoles, buff.AffectedRoles) &&
                   EqualityComparer<Func<bool>>.Default.Equals(Condition, buff.Condition) &&
                   IsConditionMet == buff.IsConditionMet;
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
            hashCode = hashCode * -1521134295 + EqualityComparer<Func<bool>>.Default.GetHashCode(Condition);
            hashCode = hashCode * -1521134295 + IsConditionMet.GetHashCode();
            return hashCode;
        }
    }
}