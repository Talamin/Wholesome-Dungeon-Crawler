using robotManager.Helpful;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;

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

        public DangerZone(IWoWUnit unit, DangerSpell spell)
        {
            Position = unit.WowUnit.Position;
            Rotation = unit.WowUnit.Rotation;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
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
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
            ObjectType = WoWObjectType.Unit;
            Danger = buff;
            Timer = new Timer(duration);
            Type = DangerType.Buff;
        }

        public bool PositionInDangerZone(Vector3 position, int margin = 0)
        {
            return Danger.PositionInDanger(position, this);
        }
        // Zer0
        /*
        public void Draw()
        {
            Danger.Draw();
        }*/
    }
}
