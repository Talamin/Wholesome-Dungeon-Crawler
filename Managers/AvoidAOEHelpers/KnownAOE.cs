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

        public KnownAOE(int id, float radius, List<LFGRoles> affectedRoles)
        {
            Id = id;
            Radius = radius;
            AffectedRoles = affectedRoles;
        }
    }
}
