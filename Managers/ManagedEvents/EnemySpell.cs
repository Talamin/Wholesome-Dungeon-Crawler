using System.Collections.Generic;
using System;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using robotManager.Helpful;
using System.Drawing;
using wManager.Wow.Helpers;
using System.IO;

namespace WholesomeDungeonCrawler.Managers
{
    public class EnemySpell : IAvoidableEvent
    {
        public int UnitId { get; private set; }
        public int SpellId { get; private set; }
        public Shape Shape { get; private set; }
        public float Size { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }
        public Func<bool> Condition { get; private set; }
        public bool IsConditionMet => Condition == null || Condition();

        double pi8 = System.Math.PI/8;
        double pi4 = System.Math.PI/4;
        float MIN = 5;
        public EnemySpell(int unitId, int spellId, Shape shape, float size, List<LFGRoles> affectedRoles, Func<bool> condition = null)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Condition = condition;
        }

        public bool PositionInDanger(Vector3 playerPosition, DangerZone zone)
        {
           
            if (playerPosition.DistanceTo(zone.DangerPosition) > Size)
                return false;
            if (playerPosition.DistanceTo(zone.DangerPosition) < MIN)
                return true;
            if (Shape == Shape.Circle)
                return playerPosition.DistanceTo(zone.DangerPosition) < Size + zone.Margin;

            Vector3 position = zone.DangerPosition;
            double angle = (double)zone.Rotation;
            float dx = playerPosition.X - position.X;
            float dy = playerPosition.Y - position.Y;
            double playerAngle = System.Math.Atan2(dy, dx);
            return Shape switch
            {
                Shape.Cone45 => (angle - pi8 < playerAngle) && (playerAngle < angle + pi8),
                Shape.Cone90 => (angle - pi4 < playerAngle) && (playerAngle < angle + pi4),
                _ => playerPosition.DistanceTo(zone.DangerPosition) < Size + zone.Margin,
            };
        }

        public void Draw(Vector3 dangerPosition, Color color, bool filled, int alpha)
        {
            Radar3D.DrawCircle(dangerPosition, Size, color, filled, alpha);            
        }
    }
}