using robotManager.Helpful;
using System.Collections.Generic;
using System.Data.Common;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;

namespace WholesomeDungeonCrawler.Managers.ManagedEvents
{
    public static class DangerList
    {
        private static readonly List<LFGRoles> _everyone = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _meleeOnly = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.MDPS };
        private static readonly List<LFGRoles> _rangedOnly = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _everyoneExceptTank = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };

        public static readonly List<KnownAOE> GetKnownAOEs = new List<KnownAOE>()
        { 
            // Classic
            // Cloud of Disease (Scholomance)
            new KnownAOE(17742, 8f, _everyone),
            // Creeping Sludge (Foulspore Caverns)
            new KnownAOE(12222, 10f, _everyone, extraMargin: 10),
            // Noxious Slime gas (Foulspore Caverns)
            new KnownAOE(21070, 8f, _everyone),
            // TBC
            // Proximity Mine (The Blood Furnace)
            new KnownAOE(181877, 12f, _everyoneExceptTank),
            // Liquid Fire (Hellfire Ramparts, last boss)   
            new KnownAOE(181890, 8f, _everyone),
            // Broggok poison cloud (Blood Furnace)
            new KnownAOE(17662, 15f, _everyone),
            // Underbog Mushroom (Underbog, Hungarfen boss)
            new KnownAOE(17990, 10f, _everyone),
            // Focus Target Visual (Mana Tombs, Shirrak the Dead Watcher)
            new KnownAOE(32286, 16f, _everyone),
            // Shirrak the Dead Watcher (cast debuff when close)
            new KnownAOE(18371, 15f, _rangedOnly),
            // Lightning Cloud (Hydromancer Thepias)
            new KnownAOE(25033, 12f, _everyone),
            // Arcane Sphere (Kael'thas Sunstrider)
            new KnownAOE(24708, 20f, _everyone),
            // Flame Strike (Kael'thas Sunstrider)
            new KnownAOE(24666, 10f, _everyone),
            // WOTLK
            // Axe - Ingvar the Plunderer (Utgarde Keep)
            new KnownAOE(23997, 7f, _everyone),            
            // Impale - Anub Arak (Azjol Nerub)
            new KnownAOE(29184, 5f, _everyone),                      
            // Freezing Cloud - Skadi (Utgarde Pinacle)
            new KnownAOE(60020, 7f, _everyone),
            // Living Mojo Puddle - Drakkari Colossus (Gundrak)
            new KnownAOE(59451, 3f, _everyone),
             // Blizzard - Novos (Drak Tharon Keep)
            new KnownAOE(49034, 9f, _everyone),
            // Blizzard - Novos (Drak Tharon Keep Heroic)
            new KnownAOE(59854, 9f, _everyone),
            
            // Exploding Orb - Krick (Pit of Saron)
            new KnownAOE(36610, 10f, _everyone),
            // Icy Blast - Scourgelord Tyranos (Pit of Saron)
            new KnownAOE(69232, 15f, _everyone),
            // Toxic Waste - Krik (Pit of Saron)
            new KnownAOE(70436, 7f, _everyone),
            // Blight Bomb - Plagueborn Horror (Pit of Saron)
            new KnownAOE(69582, 7f, _everyone),
            // Well Of Souls - Devourer of Souls (Forge of Souls)
            new KnownAOE(36536, 4f, _everyone),
            // Blizzard - Cyanigosa (Violet Hold)
            new KnownAOE(58693, 10f, _everyone),

            // Flame Sphere - Prince Taldarim (Old Kingdom)
            new KnownAOE(31686, 10f, _everyone),

            // Spark of Ionar - Ionar (Halls of Lightning
            new KnownAOE(28926, 5f, _everyone),
            
        };

 
        public static readonly List<DangerSpell> GetEnemySpells = new List<DangerSpell>()
        {
            // Azure Magus, Frostbolt (the nexus) test spell
            //new DangerSpell(26722, 56775, Shape.Cone90, 10, _everyone, 2),            
            // Slad'ran - Poison Nova, (Gundrak)
            new DangerSpell(29304, 59842, Shape.Circle, 15, _everyone, 3.5),
            new DangerSpell(26631, 55081, Shape.Circle, 15, _everyone, 3.5),
            // Drakari Elemental - Surge (Gundrak)
            new DangerSpell(29573, 54801, Shape.Cone45, 20, _everyone, 3),

            
            // Ingvar the Plunderer - Dark Smash, (Utgarde Pinacle, Heroic)
            new DangerSpell(23954, 59709, Shape.Cone90, 30, _everyone, 4),
            // Ingvar the Plunderer - Dark Smash (Utgarde Pinacle, Normal)
            new DangerSpell(23954, 42723, Shape.Cone90, 30, _everyone, 4),

             // Ingvar the Plunderer - Smash, (Utgarde Pinacle, Heroic)
            new DangerSpell(23954, 59706, Shape.Cone90, 30, _everyone, 4),
             // Ingvar the Plunderer - Smash (Utgarde Pinacle, Normal)
            new DangerSpell(23954, 42669, Shape.Cone90, 30, _everyone, 4),

              // King Ymiron - Bane (Utgarde Keep, Heroic)
            new DangerSpell(26861, 48294, Shape.Circle, 10, _everyone, 8),
            
            

            //  Ick - Poison Nova (Pit of Saron)
            new DangerSpell(36476, 70434, Shape.Circle, 20, _everyone, 5 ),
            //  Ick - Explosive Barrage (Pit of Saron)
            new DangerSpell(36476, 69012, Shape.Circle, 6, _everyone, 18 ),

            // Conjure Flame Sphere - Prince Taldarim (Old Kingdom)
            new DangerSpell(29308, 55931, Shape.Circle, 18, _everyone, 5 ),
            // Thunder Shock - Jedoga Shadowseeker (Old Kingdom)
            new DangerSpell(29310, 56926, Shape.Circle, 18, _everyone, 5 ),            
        };

        public static readonly List<DangerBuff> GetEnemyBuffs = new List<DangerBuff>()
        { 
            // Azure Warder, Mana shield (the nexus) test spell
           // new DangerBuff(26716, 56778, "Mana Shield", Shape.Circle, 10, _everyone ),
            
            // Gundrak - Gal'Drath - Whirling Slash
            new DangerBuff(29306, 59824, "Whirling Slash", Shape.Circle, 7, _everyone ),
            
            // Arcane Field - Novos - Drak Tharon Keep
            new DangerBuff(26631, 47346, "Arcane Field", Shape.Circle, 12, _everyone ),
            
            // Utgarde Pinacle - Skadi the ruthless - Whirlwind
            new DangerBuff(26693, 50228, "Whirlwind", Shape.Circle, 7, _everyone ),

            // Utgarde Pinacle - King Ymiron - Bane
            new DangerBuff(26861, 48294, "Bane", Shape.Circle, 12, _everyone ),
            
            // Genral Bjarngrim - Halls of Lightning - Whirlwind
            new DangerBuff(28586, 50227, "Whirlwind", Shape.Circle, 7, _everyone ),

            // Pit of Saron - Ick - Pursuit 
            new DangerBuff(36476, 68987, "Pursuit", Shape.Circle, 10, _everyone ),
        };

        public static readonly List<DangerDebuff> GetEnemyDebuffs = new List<DangerDebuff>()
        {
            new DangerDebuff(28546, 52658, "Static Overload", Shape.Circle, 10, _everyone )
            //  
        };

        
        public static List<ForcedSafeZone> GetForcedSafeZones = new List<ForcedSafeZone>()
        { 
            // Shirrak fight - Auchenai Crypt  
            new ForcedSafeZone(18371, new Vector3(-51.94074, -163.6697, 26.36175, "None"), 40),

            // Bronjam Forge of Souls
            new ForcedSafeZone(36497, new Vector3(5297.31, 2506.46, 686.0678, "None"), 8),
        };
    }
}
