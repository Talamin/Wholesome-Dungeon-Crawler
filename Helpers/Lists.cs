using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Models;
using static WholesomeDungeonCrawler.Helpers.TargetingHelper;

namespace WholesomeDungeonCrawler.Helpers
{
    class Lists
    {
        internal static readonly List<DungeonModel> AllDungeons = new List<DungeonModel>
        {
            new DungeonModel { Name="Ragefire Chasm", DungeonId= 4,MapId=389, ContinentId=1, EntranceLoc=new Vector3(1822.122, -4430.388, -21.98243, "None") },
            new DungeonModel { Name="The Deadmines", DungeonId=6, MapId=36, ContinentId=0, EntranceLoc=new Vector3(-11207.45, 1681.354, 23.80899, "None") },
            new DungeonModel { Name="Wailing Caverns", DungeonId=1, MapId=43, ContinentId=1, EntranceLoc=new Vector3(-749.3314, -2212.979, 14.59487, "None") },
            new DungeonModel { Name="Shadowfang Keep", DungeonId=8, MapId=33, ContinentId=0, EntranceLoc=new Vector3(-231.1921, 1571.913, 76.8921, "None") },
            new DungeonModel { Name="Blackfathom Deeps", DungeonId=10, MapId=48, ContinentId=1, EntranceLoc=new Vector3(4249.812, 750.041, -23.03586, "None")},
            new DungeonModel { Name="Stormwind Stockade", DungeonId=12, MapId=34, ContinentId=0, EntranceLoc=new Vector3(-8761.981, 847.7866, 86.25107, "None") },
            new DungeonModel { Name="Gnomeregan", DungeonId=14, MapId=90, ContinentId=0, EntranceLoc=new Vector3(-5162.39, 933.34, 257.1808, "None") },
            new DungeonModel { Name="Scarlet Monastery - Graveyard", DungeonId=18, MapId=189, ContinentId=0, EntranceLoc=new Vector3(2922.188, -798.9219, 160.333, "None") },
            new DungeonModel { Name="Scarlet Monastery - Library", DungeonId=165, MapId=189, ContinentId=0, EntranceLoc=new Vector3(2861.397, -824.2118, 160.333, "None") },
            new DungeonModel { Name="Scarlet Monastery - Armory", DungeonId=163, MapId=189, ContinentId=0, EntranceLoc=new Vector3(2877.177, -840.0958, 160.3271, "None") },
            new DungeonModel { Name="Scarlet Monastery - Cathedral", DungeonId=164, MapId=189, ContinentId=0, EntranceLoc=new Vector3(2923.722, -820.2943, 160.3281, "None") },
            new DungeonModel { Name="Razorfen Kraul", DungeonId=16, MapId=47, ContinentId=1, EntranceLoc=new Vector3(-4459.325, -1657.642, 81.80143, "None") },
            new DungeonModel { Name="Razorfen Downs", DungeonId=20, MapId=129, ContinentId=1, EntranceLoc=new Vector3(-4662.351, -2533.959, 82.09897, "None")},
            new DungeonModel { Name="Uldaman", DungeonId=22, MapId=70, ContinentId=0, EntranceLoc=new Vector3(-6059.759, -2955.001, 209.769, "None") },
            new DungeonModel { Name="Maraudon - Foulspore Cavern", DungeonId=26, MapId=349, ContinentId=1, EntranceLoc=new Vector3(-1478.906, 2616.896, 75.71483, "None") }, // orange
            new DungeonModel { Name="Maraudon - The Wicked Grotto", DungeonId=272, MapId=349, ContinentId=1, EntranceLoc=new Vector3(-1184.009, 2868.457, 85.56033, "None") }, // purple
            new DungeonModel { Name="Maraudon - Earth Song Falls", DungeonId=273, MapId=349, ContinentId=1, EntranceLoc=new Vector3(-1184.009, 2868.457, 85.56033, "None") },
            new DungeonModel { Name="Zul'Farrak", DungeonId=24, MapId=209, ContinentId=1, EntranceLoc=new Vector3(-6790.196, -2891.261, 8.902938, "None") },
            new DungeonModel { Name="The Temple of Atal'Hakkar", DungeonId=28, MapId=109, ContinentId=0, EntranceLoc=new Vector3(-10169.46, -3997.184, -113.8935, "None") },
            new DungeonModel { Name="Blackrock Depths - Prison", DungeonId=30, MapId=230, ContinentId=0, EntranceLoc=new Vector3(-7177.132, -932.0781, 165.9823, "None") },
            new DungeonModel { Name="Blackrock Depths - Upper City", DungeonId=276, ContinentId=0, MapId=230, EntranceLoc=new Vector3(-7177.132, -932.0781, 165.9823, "None") },
            new DungeonModel { Name="Blackrock Spire", DungeonId=32, MapId=229, ContinentId=0, EntranceLoc=new Vector3(-7522.83, -1233.157, 285.7446, "None") },
            new DungeonModel { Name="Upper Blackrock Spire", DungeonId=44, MapId=229, ContinentId=0, EntranceLoc=new Vector3(-7522.83, -1233.157, 285.7446, "None") },
            new DungeonModel { Name="Dire Maul West", DungeonId=36, MapId=429, ContinentId=1, EntranceLoc=new Vector3(-3834.753, 1250.436, 160.226, "None") },
            new DungeonModel { Name="Dire Maul North", DungeonId=38, MapId=429, ContinentId=1, EntranceLoc=new Vector3(-3520.973, 1073.685, 161.1654, "None") },
            new DungeonModel { Name="Dire Maul East", DungeonId=34, MapId=429, ContinentId=1, EntranceLoc=new Vector3(-3732.743, 934.5352, 160.9966, "None") },
            new DungeonModel { Name="Scholomance", DungeonId= 2, MapId=289, ContinentId=0, EntranceLoc=new Vector3(1280.767, -2549.763, 86.29783, "None") },
            new DungeonModel { Name="Stratholme - Main Gate", DungeonId=40, MapId=329, ContinentId=0, EntranceLoc=new Vector3(3237.718, -4055.83, 108.4676, "None") },
            new DungeonModel { Name="Stratholme - Service Entrance", DungeonId=274, MapId=329, ContinentId=0, EntranceLoc=new Vector3(3237.718, -4055.83, 108.4676, "None") },
            new DungeonModel { Name="Hellfire Ramparts", DungeonId=136, MapId=543, ContinentId=530, EntranceLoc=new Vector3(-365.8618, 3087.007, -14.58022, "None") },
            new DungeonModel { Name="The Blood Furnace", DungeonId=137, MapId=542, ContinentId=530, EntranceLoc=new Vector3(-307.8876, 3171.705, 28.40893, "None") },
            new DungeonModel { Name="The Slave Pens", DungeonId=140, MapId=547, ContinentId=530, EntranceLoc=new Vector3(741.4343, 7011.924, -73.0747, "None") },
            new DungeonModel { Name="The Underbog", DungeonId=146, MapId=546, ContinentId=530, EntranceLoc=new Vector3(783.4067, 6742.714, -72.53995, "None") },
            new DungeonModel { Name="Mana Tombs", DungeonId=148, MapId=557, ContinentId=530, EntranceLoc=new Vector3(-3069.809, 4943.236, -101.0472, "None") },
            new DungeonModel { Name="Auchenai Crypts", DungeonId=149, MapId=558, ContinentId=530, EntranceLoc=new Vector3(-3361.442, 5233.683, -101.0484, "None") },
            new DungeonModel { Name="Old Hillsbrad Foothills", DungeonId=170, MapId=560, ContinentId=530, EntranceLoc=new Vector3(-8330.366, -4054.07, -207.6699, "None") },
            new DungeonModel { Name="Sethekk Halls", DungeonId=150, MapId=556, ContinentId=530, EntranceLoc=new Vector3(-3361.055, 4652.112, -101.0487, "None") },
            new DungeonModel { Name="The Steamvault", DungeonId=147, MapId=545, ContinentId=530, EntranceLoc=new Vector3(816.4943, 6952.726, -80.54607, "None") },
            new DungeonModel { Name="Shadow Labyrinth", DungeonId=151, MapId=555, ContinentId=530, EntranceLoc=new Vector3(-3653.643, 4944.552, -101.3898, "None") },
            new DungeonModel { Name="The Arcatraz", DungeonId=174, MapId=552, ContinentId=530, EntranceLoc=new Vector3(3313.221, 1328.768, 505.5585, "Flying") },
            new DungeonModel { Name="The Black Morass", DungeonId=171, MapId=269, ContinentId=530, EntranceLoc=new Vector3(-8768.379, -4164.33, -210.2725, "None") },
            new DungeonModel { Name="The Botanica", DungeonId=173, MapId=553, ContinentId=530, EntranceLoc=new Vector3(3417.226, 1480.172, 182.8366, "Flying") },
            new DungeonModel { Name="The Mechanar", DungeonId=172, MapId=554, ContinentId=530, EntranceLoc=new Vector3(2860.551, 1544.211, 252.159, "Flying") },
            new DungeonModel { Name="The Shattered Halls", DungeonId=138, MapId=540, ContinentId=530, EntranceLoc=new Vector3(-365.8618, 3087.007, -14.58022, "None") },
            new DungeonModel { Name="Magisters' Terrace", DungeonId=198, MapId=585, ContinentId=530, EntranceLoc=new Vector3(12881.7, -7342.356, 65.52691, "None") },
            new DungeonModel { Name="Utgarde Keep", DungeonId=202, MapId=574, ContinentId=571, EntranceLoc=new Vector3(1236.232, -4859.632, 41.24857, "None") },
            new DungeonModel { Name="The Nexus", DungeonId=225, MapId=576, ContinentId=571, EntranceLoc=new Vector3(3906.412, 6985.285, 69.4881, "None") },
            new DungeonModel { Name="Azjol Nerub", DungeonId=204, MapId=601, ContinentId=571, EntranceLoc=new Vector3(3669.932, 2173.066, 36.05176, "None") },
            new DungeonModel { Name="Ahn'kahet The Old Kingdom", DungeonId=218, MapId=619, ContinentId=571, EntranceLoc=new Vector3(3639.375, 2026.671, 2.541712, "None") },
            new DungeonModel { Name="Drak'Tharon Keep", DungeonId=214, MapId=600, ContinentId=571, EntranceLoc=new Vector3(4774.752, -2018.308, 229.394, "None") },
            new DungeonModel { Name="The Violet Hold", DungeonId=220, MapId=608, ContinentId=571, EntranceLoc=new Vector3(5675.277, 479.8194, 652.2078, "None") },
            new DungeonModel { Name="Gundrak", DungeonId=216, MapId=604, ContinentId=571, EntranceLoc=new Vector3(6972.771, -4399.461, 441.576, "None") },
            new DungeonModel { Name="Halls of Stone", DungeonId=208, MapId=599, ContinentId=571, EntranceLoc=new Vector3(8920.499, -962.8192, 1039.132, "Flying") },
            new DungeonModel { Name="Halls of Lightning", DungeonId=207, MapId=602, ContinentId=571, EntranceLoc=new Vector3(9192.92, -1388.867, 1110.215, "Flying") },
            new DungeonModel { Name="The Oculus", DungeonId=206, MapId=578, ContinentId=571, EntranceLoc=new Vector3(3869.969, 6984.653, 108.1261, "None") },
            new DungeonModel { Name="The Culling of Stratholme", DungeonId=209, MapId=595, ContinentId=571, EntranceLoc=new Vector3(-8757.122, -4466.833, -201.2474, "None") },
            new DungeonModel { Name="Utgarde Pinnacle", DungeonId=203, MapId=575, ContinentId=571, EntranceLoc=new Vector3(1230.303, -4861.86, 218.289, "None") },
            new DungeonModel { Name="The Forge of Souls", DungeonId=251, MapId=632, ContinentId=571, EntranceLoc=new Vector3(5675.031, 1998.231, 798.0471, "None") },
            new DungeonModel { Name="Pit of Saron", DungeonId=253, MapId=658, ContinentId=571, EntranceLoc=new Vector3(5586.841, 2006.409, 798.0458, "None") },
            new DungeonModel { Name="Halls of Reflection", DungeonId=255, MapId=668, ContinentId=571, EntranceLoc=new Vector3(5626.953, 1964.744, 803.021, "None") },
            new DungeonModel { Name="Trial of the Champion", DungeonId=245, MapId=650, ContinentId=571, EntranceLoc=new Vector3(8567.237, 791.7763, 558.5429, "None") },

            // Heroic
            new DungeonModel { Name="Utgarde Keep", IsHeroic = true, DungeonId=242, MapId=574, ContinentId=571, EntranceLoc=new Vector3(1236.232, -4859.632, 41.24857, "None") },
            new DungeonModel { Name="Utgarde Pinnacle", IsHeroic = true, DungeonId=205, MapId=575, ContinentId=571, EntranceLoc=new Vector3(1230.303, -4861.86, 218.289, "None") },
            new DungeonModel { Name="The Oculus", IsHeroic = true, DungeonId=211, MapId=578, ContinentId=571, EntranceLoc=new Vector3(3869.969, 6984.653, 108.1261, "None") },
            new DungeonModel { Name="Halls of Stone", IsHeroic = true, DungeonId=213, MapId=599, ContinentId=571, EntranceLoc=new Vector3(8920.499, -962.8192, 1039.132, "Flying") },
            new DungeonModel { Name="Drak'Tharon Keep", IsHeroic = true, DungeonId=215, MapId=600, ContinentId=571, EntranceLoc=new Vector3(4774.752, -2018.308, 229.394, "None") },
            new DungeonModel { Name="Gundrak", IsHeroic = true, DungeonId=217, MapId=604, ContinentId=571, EntranceLoc=new Vector3(6972.771, -4399.461, 441.576, "None") },
            new DungeonModel { Name="Ahn'kahet The Old Kingdom", IsHeroic = true, DungeonId=219, MapId=619, ContinentId=571, EntranceLoc=new Vector3(3639.375, 2026.671, 2.541712, "None") },
            new DungeonModel { Name="The Violet Hold", IsHeroic = true, DungeonId=221, MapId=608, ContinentId=571, EntranceLoc=new Vector3(5675.277, 479.8194, 652.2078, "None") },
            new DungeonModel { Name="Azjol Nerub", IsHeroic = true, DungeonId=241, MapId=601, ContinentId=571, EntranceLoc=new Vector3(3669.932, 2173.066, 36.05176, "None") },
            new DungeonModel { Name="Trial of the Champion", IsHeroic = true, DungeonId=249, MapId=650, ContinentId=571, EntranceLoc=new Vector3(8567.237, 791.7763, 558.5429, "None") },
            new DungeonModel { Name="The Culling of Stratholme", DungeonId=210, MapId=595, ContinentId=571, EntranceLoc=new Vector3(-8757.122, -4466.833, -201.2474, "None") },
            new DungeonModel { Name="Halls of Lightning", IsHeroic = true, DungeonId=212, MapId=602, ContinentId=571, EntranceLoc=new Vector3(9192.92, -1388.867, 1110.215, "Flying") },
            new DungeonModel { Name="The Nexus", IsHeroic = true, DungeonId=226, MapId=576, ContinentId=571, EntranceLoc=new Vector3(3906.412, 6985.285, 69.4881, "None") },

            // Raids
            new DungeonModel { Name="Molten Core", IsRaid = true, MapId=409 },

        }.OrderBy(dungeon => dungeon.Name).ToList();

        // These mobs will only be ignored during MoveAlongPath/DefendSpot/Pull/Escort checks (they won't be pulled).
        // The group will still defend against them if attacked.
        public static readonly HashSet<int> MobsToIgnoreDuringSteps = new HashSet<int>
        {
            26793, // Crystalline Frayer (Nexus)
            191016, // Seed Pod (Nexus)
            32593, // Skittering Swarmer Anub'arak            
            2748, // Archaedas
            10120, // Vault Warder
            4857, // Stone Keeper
            7309, // Earthen Custodian
            7077, // Earthen Hallshaper
            7076, // Earthen Guardian
            17822, // Landen Stilwell (SFK Jail)
            3850, // Sorcerer Ashcrombe (SFK Jail)
            3849, // Deathstalker Adamant (SFK Jail)
            8130, // Sul'lithuz Hatchling (ZF)
            10096, // High Justice Grimstone (BRD)
            29834, // Drakkari Frenzy (Gundrak)
        };

        // These neutral mobs will be pulled when navigating the dungeon as if they were hostile
        public static readonly HashSet<int> NeutralsToAttackDuringPathCheck = new HashSet<int>
        {
            4625, // Death's Head Ward Keeper (RFK)
            3653, // Kresh (Wailing Caverns)
            //9499, // Plugger Spazzring (BRD)
        };

        // These mobs will have a different target priority (only during fights and if they are actively engaged with your team)
        public static readonly Dictionary<int, SpecialPrio> SpecialPrioTargets = new Dictionary<int, SpecialPrio>
        {
            // High
            // Classic
            { 23775, new SpecialPrio(23682, TargetPriority.High) }, // Head of the Horseman (Haloween Event)
            { 598, new SpecialPrio(0, TargetPriority.High, true) }, // Defias Miner (Deadmines)
            { 11582, new SpecialPrio(0, TargetPriority.High, true) }, // Dark Summoner (Scholomance)
            { 4292, new SpecialPrio(0, TargetPriority.High, true) }, // Scarlet Protector (Armory)
            { 4303, new SpecialPrio(0, TargetPriority.High, true) }, // Scarlet Abott (Cathedral)
            // BC
            { 18176, new SpecialPrio(17941, TargetPriority.High) }, // Tainted Earthgrab Totem (Mennu, Slave Pens)
            { 20208, new SpecialPrio(17941, TargetPriority.High) }, // Mennu's Healing Ward (Mennu, Slave Pens)
            { 18177, new SpecialPrio(17941, TargetPriority.High) }, // Tainted Stoneskin Totem (Mennu, Slave Pens)
            { 18179, new SpecialPrio(17941, TargetPriority.High) }, // Corrupted Nova Totem (Mennu, Slave Pens)                        
            { 17917, new SpecialPrio(17797, TargetPriority.High) }, // Coilfang Water Elemental (Hydromancer Thespia, Steamvault)
            { 17951, new SpecialPrio(17796, TargetPriority.High) }, // Steamrigger Mechanic (Mekgineer SteamRigger, Steamvault)
            { 17954, new SpecialPrio(17798, TargetPriority.High) }, // Naga Distiller (Warlord Kalithresh, Steamvault)
            { 24722, new SpecialPrio(24723, TargetPriority.High) }, // Fell Crystal (Selin FIreheart, Magister's Terrace)
            { 24675, new SpecialPrio(24664, TargetPriority.High) }, // Phoenix egg (Kael'thas Sunstrider, Magister's Terrace)
            { 24674, new SpecialPrio(24664, TargetPriority.High) }, // Phoenix (Kael'thas Sunstrider, Magister's Terrace)            
            // Wotlk
            { 5650, new SpecialPrio(0, TargetPriority.High, true) }, // Sandfury Witch Doctor (Zul'Farak)
            { 26918, new SpecialPrio(26763, TargetPriority.High) }, // Chaotic Rift (Anomalus, Nexus)
            { 30176, new SpecialPrio(29309, TargetPriority.High) }, // Ahn'kahar Guardian (Elder Nadox, Old Kingdom)
            { 30385, new SpecialPrio(29310, TargetPriority.High) }, // Twilight Volunteer (Jedoga Shadowseer, Old Kingdom)
            { 28619, new SpecialPrio(0, TargetPriority.High) }, // Web Wrap (Krik'thir, Azjol Nerub)
            { 28734, new SpecialPrio(28684, TargetPriority.High) }, // Anubar Skirmisher (Krik'thir, Azjol Nerub)
            { 23965, new SpecialPrio(23953, TargetPriority.High) }, // Frost Tomb (Prince Kelesth, Utgard Keep)
            { 36535, new SpecialPrio(36497, TargetPriority.High) }, // Corrupt Soul (Bronjahm , Forge of Souls)
            

            { 14324, new SpecialPrio(11501, TargetPriority.High) }, // Cho'Rush the Observer (DM North)
            { 17540, new SpecialPrio(0, TargetPriority.High, true) }, // Fiendish Hound (Hellfire Ramparts)
            { 17537, new SpecialPrio(0, TargetPriority.High, true) }, // Vazruden (Hellfire Ramparts)
            { 17309, new SpecialPrio(0, TargetPriority.High, true) }, // Hellfire Watcher (Hellfire Ramparts)
            // Low
            { 8996, new SpecialPrio(0, TargetPriority.Low, true) }, // Voidwalker minion (Ragefire Chasm)
            { 2520, new SpecialPrio(0, TargetPriority.Low, true) }, // Remote-Controlled Golem (Deadmines)
            { 8317, new SpecialPrio(0, TargetPriority.Low, true) }, // Atal'ai Deathwalker's Spirit (Temple)
    }   ;

        // These mobs will be completely ignored in the entity cache.
        // Careful with this one, the group won't even defend against them.
        public static readonly HashSet<int> IgnoredMobs = new HashSet<int>
        {
            191016, // Seed Pod (Nexus)
        };
    }
}
