using robotManager.Helpful;
using System.Drawing;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    public class DangerZone
    {
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public ulong Guid { get; private set; }
        public string Name { get; private set; }
        public WoWObjectType ObjectType { get; private set; }
        public double Rotation { get; private set; }
        public DangerType Type { get; private set; }
        public IAvoidableEvent Danger { get; private set; }
        public Timer Timer { get; private set; }

        public DangerZone(DangerObject dangerObject)
        {
            Position = dangerObject.WowObject.Position;
            Guid = dangerObject.WowObject.Guid;
            Name = string.IsNullOrEmpty(dangerObject.WowObject.Name) ? "Unknown object" : dangerObject.WowObject.Name;
            ObjectType = dangerObject.WowObject.Type;
            Radius = dangerObject.Size;
            Type = DangerType.GameObject;
            Danger = dangerObject;
        }

        public DangerZone(IWoWUnit unit, DangerSpell spell, string spellName)
        {
            Position = unit.WowUnit.Position;
            Rotation = unit.WowUnit.Rotation;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(spellName) ? "Unknown spell" : spellName;
            ObjectType = WoWObjectType.Unit;
            Danger = spell;
            Timer = new Timer(spell.Duration * 1000);
            Type = DangerType.Spell;
        }

        public DangerZone(IWoWUnit unit, DangerBuff buff, double duration)
        {
            Position = unit.WowUnit.Position;
            Rotation = unit.WowUnit.Rotation;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(buff.Name) ? "Unknown buff" : buff.Name;
            ObjectType = WoWObjectType.Unit;
            Danger = buff;
            Timer = new Timer(duration);
            Type = DangerType.Buff;
        }

        public bool PositionInDangerZone(Vector3 position, int margin = 0)
        {
            return Danger.PositionInDanger(position, this);
        }
        
        public void Draw()
        {
            if (Danger is DangerBuff)
            {
                Radar3D.DrawCircle(Position, Radius, Color.Orange, false, 30);
            }
            else if (Danger is DangerSpell dangerSpell)
            {
                double x = Position.X + dangerSpell.Size * System.Math.Sin(Rotation);
                double y = Position.Y + dangerSpell.Size * System.Math.Cos(Rotation);
                double rt2 = System.Math.Sqrt(2);
                double z = Position.Z;
                switch (dangerSpell.Shape)
                {
                    case Shape.Cone90:
                        double topX = (x - y) / rt2;
                        double topY = (x + y) / rt2;
                        Vector3 topLinePosition = new Vector3(topX, topY, z);
                        double botX = topY; // Maths works out the same 
                        double botY = (y - x) / rt2;
                        Vector3 bottomLinePosition = new Vector3(botX, botY, z);
                        Radar3D.DrawLine(Position, topLinePosition, Color.White, 200);
                        Radar3D.DrawLine(Position, bottomLinePosition, Color.White, 200);
                        break;
                    case Shape.Circle:
                    default:
                        Radar3D.DrawCircle(Position, dangerSpell.Size, Color.Purple, false, 30);
                        break;
                }
            }
            else if (Danger is DangerObject)
            {
                Radar3D.DrawCircle(Position, Radius, Color.Red, false, 30);
            }
        }
    }
}
