using System;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    public class KnownAOE
    {
        public int Id { get; private set; }
        public float Radius { get; private set; }
        public List<LFGRoles> AffectedRoles { get; private set; }
        public Func<bool> Condition { get; private set; }
        public bool IsConditionMet => Condition == null || Condition();

        public KnownAOE(int id, float radius, List<LFGRoles> affectedRoles, Func<bool> condition = null)
        {
            Id = id;
            Radius = radius;
            AffectedRoles = affectedRoles;
            Condition = condition;
        }
    }
}
