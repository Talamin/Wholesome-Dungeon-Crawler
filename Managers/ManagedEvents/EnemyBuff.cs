using System.Collections.Generic;
using System;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    public class EnemyBuff
    {
        public int UnitId { get; private set; }
        public int SpellId { get; private set; }
        public Shape Shape { get; private set; }
        public float Size { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }
        public Func<bool> Condition { get; private set; }
        public bool IsConditionMet => Condition == null || Condition();

        public EnemyBuff(int unitId, int spellId, Shape shape, float size, List<LFGRoles> affectedRoles, Func<bool> condition = null)
        {
            UnitId = unitId;
            SpellId = spellId;
            Shape = shape;
            Size = size;
            AffectedRoles = affectedRoles;
            Condition = condition;
        }
    }
}