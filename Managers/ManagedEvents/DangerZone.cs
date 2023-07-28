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
        public float Rotation { get; private set; }
        public ulong Guid { get; private set; }
        public string Name { get; private set; }
        public WoWObjectType ObjectType { get; private set; }            

        public IAvoidableEvent Danger;

        public int Margin;

        public DangerZone(WoWObject wowObject, float radius, int margin = 0)
        {
            DangerPosition = wowObject.Position;
            Guid = wowObject.Guid;
            Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
            ObjectType = wowObject.Type;
            Radius = radius;
            Margin = margin;
                
        }
        public DangerZone(IWoWUnit unit, EnemySpell spell, int margin = 0)
        {
            DangerPosition = unit.WowUnit.Position;
            Rotation = unit.WowUnit.Rotation;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
            ObjectType = WoWObjectType.Unit;                
            Danger = spell;
            Margin = margin;
        }
        public DangerZone(IWoWUnit unit, EnemyBuff buff, int margin = 0)
        {
            DangerPosition = unit.WowUnit.Position;
            Rotation = unit.WowUnit.Rotation;
            Guid = unit.Guid;
            Name = string.IsNullOrEmpty(unit.Name) ? "Unknown object" : unit.Name;
            ObjectType = WoWObjectType.Unit;
            Danger = buff;
            Margin = margin;

        }
        public bool PositionInDangerZone(Vector3 playerPosition, int margin = 0)
        {
            this.Margin= margin;
            return Danger.PositionInDanger(playerPosition, this);
        }

        public void Draw(Color color, bool filled, int alpha)
        {
            Danger.Draw(DangerPosition, color, filled, alpha);
        }       
    }
}
