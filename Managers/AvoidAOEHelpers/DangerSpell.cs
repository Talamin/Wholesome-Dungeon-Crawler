using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;

namespace WholesomeDungeonCrawler.Managers
{
    public class DangerSpell : IAvoidableEvent
    {
        public int UnitId { get; private set; }
        public int SpellId { get; private set; }
        public Shape Shape { get; private set; }
        public float Size { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }
        public double Duration { get; private set; }

        static readonly double pi8 = System.Math.PI / 8;
        static readonly double pi4 = System.Math.PI / 4;
        static readonly double pi2 = System.Math.PI / 4;
        //static readonly double rt2 = System.Math.Sqrt(2);
        static readonly float MIN = 1;

        public DangerSpell(int unitId, int spellId, Shape shape, float size, List<LFGRoles> affectedRoles, double duration)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Duration = duration;
        }

        public bool PositionInDanger(Vector3 playerPosition, DangerZone zone, int margin = 0)
        {
            if (playerPosition.DistanceTo(zone.Position) > Size)
                return false;
            if (playerPosition.DistanceTo(zone.Position) < MIN)
                return true;
            if (Shape == Shape.Circle)
                return playerPosition.DistanceTo(zone.Position) < Size;

            Vector3 position = zone.Position;
            double angle = (double)zone.Rotation;
            float dx = playerPosition.X - position.X;
            float dy = playerPosition.Y - position.Y;
            double playerAngle = System.Math.Atan2(dy, dx);
            return Shape switch
            {
                Shape.Cone45 => (angle - pi8 < playerAngle) && (playerAngle < angle + pi8),
                Shape.Cone90 => (angle - pi4 < playerAngle) && (playerAngle < angle + pi4),
                Shape.Cone180 => (angle - pi2 < playerAngle) && (playerAngle < angle + pi2),
                _ => playerPosition.DistanceTo(zone.Position) < Size,
            };
        }
        /*
        public void Draw(Vector3 dangerPosition, DangerZone zone, Color color, bool filled, int alpha)
        {
            if (!WholesomeDungeonCrawlerSettings.CurrentSetting.EnableDevMode) return;

            double x = dangerPosition.X + Size * System.Math.Sin(zone.Rotation);
            double y = dangerPosition.Y + Size * System.Math.Cos(zone.Rotation);
            double z = dangerPosition.Z;
            switch (Shape)
            {
                case Shape.Cone90:
                    double topX = (x - y) / rt2;
                    double topY = (x + y) / rt2;
                    Vector3 topLinePosition = new Vector3(topX, topY, z);
                    double botX = topY; // Maths works out the same 
                    double botY = (y - x) / rt2;
                    Vector3 bottomLinePosition = new Vector3(botX, botY, z);
                    Radar3D.DrawLine(dangerPosition, topLinePosition, color, alpha);
                    Radar3D.DrawLine(dangerPosition, bottomLinePosition, color, alpha);
                    break;
                case Shape.Circle:
                default:
                    Radar3D.DrawCircle(dangerPosition, Size, color, filled, alpha);
                    break;
            }
        }
        */
        public override bool Equals(object obj)
        {
            return obj is DangerSpell spell &&
                   UnitId == spell.UnitId &&
                   SpellId == spell.SpellId &&
                   Shape == spell.Shape &&
                   Size == spell.Size &&
                   EqualityComparer<List<LFGRoles>>.Default.Equals(AffectedRoles, spell.AffectedRoles);
        }

        public override int GetHashCode()
        {
            int hashCode = -856473187;
            hashCode = hashCode * -1521134295 + UnitId.GetHashCode();
            hashCode = hashCode * -1521134295 + SpellId.GetHashCode();
            hashCode = hashCode * -1521134295 + Shape.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<LFGRoles>>.Default.GetHashCode(AffectedRoles);
            hashCode = hashCode * -1521134295 + Duration.GetHashCode();
            return hashCode;
        }
    }
}