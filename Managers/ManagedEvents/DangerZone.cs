using robotManager.Helpful;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{    
        public class DangerZone
        {
            public Vector3 Position { get; private set; }
            public float Radius { get; private set; }
            public ulong Guid { get; private set; }
            public string Name { get; private set; }
            public WoWObjectType ObjectType { get; private set; }            
            public EnemySpell Spell { get; private set; }
            public EnemyBuff Buff{ get; private set; }
            public DangerZone(WoWObject wowObject, float radius)
            {
                Position = wowObject.Position;
                Guid = wowObject.Guid;
                Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
                ObjectType = wowObject.Type;
                Radius = radius;
                
            }
            public DangerZone(IWoWUnit unit, EnemySpell spell)
            {
                Position = unit.WowUnit.Position;
                Guid = unit.Guid;
                Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
                ObjectType = WoWObjectType.Unit;                
                Spell = spell;
            }
            public DangerZone(IWoWUnit unit, EnemyBuff buff)
            {
                Position = unit.WowUnit.Position;
                Guid = unit.Guid;
                Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
                ObjectType = WoWObjectType.Unit;
                Buff = buff;
            }
            public bool PositionInDangerZone(Vector3 position, int margin = 0)
            {
            if (Spell != null)
            {
                return Position.DistanceTo(position) < Spell.Size + margin;
            } else if (Buff != null)
            {
                return Position.DistanceTo(position) < Buff.Size + margin;
            }
            else
                return Position.DistanceTo(position) < Radius + margin;
            }
        }
    }
