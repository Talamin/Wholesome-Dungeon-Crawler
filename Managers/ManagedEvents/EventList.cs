using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Managers.ManagedEvents
{
    public static class EventList
    {
        private static readonly List<LFGRoles> _everyone = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _meleeOnly = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.MDPS };
        private static readonly List<LFGRoles> _rangedOnly = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _everyoneExceptTank = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };

        public static readonly List<EnemySpell> GetEnemySpells = new List<EnemySpell>()
        {
            
            // Azure Magus, Frost bolt (the nexus) test spell
            new EnemySpell(26722, 56775,Shape.Cone45, 25, _everyone ), 
           
            // Ingvar the Plunderer, Dark Smash, Utgarde Pinacle
            new EnemySpell(23954, 59709, Shape.Cone45, 10, _everyone ),
             // Ingvar the Plunderer, Smash, Utgarde Pinacle
            new EnemySpell(23954, 59706, Shape.Cone45, 10, _everyone ),


        };

        public static readonly List<EnemyBuff> GetEnemyBuffs = new List<EnemyBuff>()
        { 
            // Azure Warder, Mana shield(the nexus) test spell
            new EnemyBuff(26716, 56778, Shape.Circle, 15, _everyone ),
            
            // Gundrak - Gal'Drath - Whirling Slash
            new EnemyBuff(29306, 59824, Shape.Circle, 7, _everyone ),
            
        };
    }
}
