using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    public class KnownAOE
    {
        public int Id { get; private set; }
        public float Radius { get; private set; }
        public int ExtraMargin { get; private set; } // keep running for extra distance
        public List<LFGRoles> AffectedRoles { get; private set; }

        public KnownAOE(int id, float radius, List<LFGRoles> affectedRoles, int extraMargin = 0)
        {
            Id = id;
            Radius = radius;
            AffectedRoles = affectedRoles;
            ExtraMargin = extraMargin;
        }
    }
}
