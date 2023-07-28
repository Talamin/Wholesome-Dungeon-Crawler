using MahApps.Metro.Controls;
using robotManager.Helpful;
using System;
using System.Drawing;
using System.Drawing.Printing;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeDungeonCrawler.Managers.AvoidAOEManager;

namespace WholesomeDungeonCrawler.Managers
{    
    public class DangerZone
    {
        public Vector3 DangerPosition { get; private set; }
        public float Radius { get; private set; }
        public ulong Guid { get; private set; }
        public string Name { get; private set; }
        public WoWObjectType ObjectType { get; private set; }            

        public IAvoidableEvent Danger;

        public DangerZone(WoWObject wowObject, float radius)
        {
            DangerPosition = wowObject.Position;
            Guid = wowObject.Guid;
            Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
            ObjectType = wowObject.Type;
            Radius = radius;
                
        }
        public DangerZone(IWoWUnit unit, EnemySpell spell)
        {
            DangerPosition = unit.WowUnit.Position;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
            ObjectType = WoWObjectType.Unit;                
            Danger = spell;
        }
        public DangerZone(IWoWUnit unit, EnemyBuff buff)
        {
            DangerPosition = unit.WowUnit.Position;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
            ObjectType = WoWObjectType.Unit;
            Danger = buff;
        }
        public bool PositionInDangerZone(Vector3 playerPosition, int margin = 0)
        {
            return Danger.PositionInDanger(playerPosition, DangerPosition, margin);
        }

        public void Draw(Color color, bool filled, int alpha)
        {
            Danger.Draw(DangerPosition, color, filled, alpha);
        }       
    }
}
