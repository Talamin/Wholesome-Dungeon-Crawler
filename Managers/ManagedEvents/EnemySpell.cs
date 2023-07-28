using System.Collections.Generic;
using System;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
using robotManager.Helpful;
using System.Drawing;
using wManager.Wow.Helpers;

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

        public EnemySpell(int unitId, int spellId, Shape shape, float size, List<LFGRoles> affectedRoles, Func<bool> condition = null)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Condition = condition;
        }

        public bool PositionInDanger(Vector3 playerPosition, Vector3 dangerPosition, int margin = 0)
        {
            return playerPosition.DistanceTo(dangerPosition) < Size + margin;
        }

        public void Draw(Vector3 dangerPosition, Color color, bool filled, int alpha)
        {
            Radar3D.DrawCircle(dangerPosition, Size, color, filled, alpha);
        }
    }
}