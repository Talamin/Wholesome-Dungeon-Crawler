using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Dungeonlogic;

namespace WholesomeDungeonCrawler.Helpers
{
    public class Dungeon
    {
        public string Name { get; set; }
        public int MapId { get; set; }
        public int DungeonId { get; set; }
        public Vector3 Start { get; set; }        
        public Vector3 EntranceLoc { get; set; }
    }

    class Lists
    {
        internal static readonly List<Dungeon> AllDungeons = new List<Dungeon>
        {
            new Dungeon { Name="Blackfathom Deeps",DungeonId =10, MapId=48, EntranceLoc=new Vector3(4249.812, 750.041, -23.03586, "None")},
            new Dungeon { Name="Blackrock Depths - Prison",DungeonId =30, MapId=230, EntranceLoc=new Vector3(-7177.132, -932.0781, 165.9823, "None"), Start=new Vector3(458.32, 26.52, -70.67339, "None")},
            new Dungeon { Name="Blackrock Depths - Upper City",DungeonId =276, MapId=230, EntranceLoc=new Vector3(-7177.132, -932.0781, 165.9823, "None"), Start=new Vector3(872.8282, -232.221, -43.75124, "None")},
            new Dungeon { Name="Blackrock Spire",DungeonId =32, MapId=229, EntranceLoc=new Vector3(-7522.83, -1233.157, 285.7446, "None") },
            new Dungeon { Name="Dire Maul West",DungeonId =36, MapId=429, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(31.5609, 159.45, -3.4777, "None") },
            new Dungeon { Name="Dire Maul North",DungeonId =38, MapId=429, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(255.249, -16.0561, -2.58737, "None") },
            new Dungeon { Name="Dire Maul East",DungeonId =34, MapId=429, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(44.4499, -154.822, -2.71364, "None")},
            new Dungeon { Name="Gnomeregan",DungeonId =14, MapId=90, EntranceLoc=new Vector3(-5162.39, 933.34, 257.1808, "None")},
            new Dungeon { Name="Maraudon - Foulspore Cavern", MapId=349, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(1019.69, -458.31, -43.43, "None")},
            new Dungeon { Name="Maraudon - The Wicked Grotto", MapId=349, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(752.91, -616.53, -33.11, "None")},
            new Dungeon { Name="Maraudon - Earth Song Falls", MapId=349, EntranceLoc=new Vector3(0, 0,0, "None"), Start=new Vector3(419.84, 11.3365, -132.194)},
            new Dungeon { Name="Ragefire Chasm", DungeonId = 4,MapId=389, EntranceLoc=new Vector3(1822.122, -4430.388, -21.98243, "None") },
            new Dungeon { Name="Razorfen Downs", DungeonId =20, MapId=129, EntranceLoc=new Vector3(-4662.351, -2533.959, 82.09897, "None")},
            new Dungeon { Name="Razorfen Kraul",DungeonId =16, MapId=47, EntranceLoc=new Vector3(-4459.325, -1657.642, 81.80143, "None") },
            new Dungeon { Name="Scarlet Monastery - Graveyard",DungeonId =18, MapId=189, EntranceLoc=new Vector3(2922.188, -798.9219, 160.333, "None"), Start=new Vector3(1688.99, 1053.48, 18.67749, "None")},
            new Dungeon { Name="Scarlet Monastery - Library",DungeonId =165, MapId=189, EntranceLoc=new Vector3(2861.397, -824.2118, 160.333, "None"), Start=new Vector3(255.346, -209.09, 18.6773, "None") },
            new Dungeon { Name="Scarlet Monastery - Armory", DungeonId =163, MapId=189, EntranceLoc=new Vector3(2877.177, -840.0958, 160.3271, "None"), Start=new Vector3(1610.83, -323.433, 18.67379, "None") },
            new Dungeon { Name="Scarlet Monastery - Cathedral",DungeonId =164, MapId=189, EntranceLoc=new Vector3(2923.722, -820.2943, 160.3281, "None"), Start=new Vector3(855.683, 1321.5, 18.6709, "None") },
            new Dungeon { Name="Scholomance",DungeonId = 2, MapId=289, EntranceLoc=new Vector3(1280.767, -2549.763, 86.29783, "None") },
            new Dungeon { Name="Shadowfang Keep",DungeonId =8, MapId=33, EntranceLoc=new Vector3(-231.1921, 1571.913, 76.8921, "None") },
            new Dungeon { Name="Stratholme - Main Gate",DungeonId =40, MapId=329, EntranceLoc=new Vector3(3237.718, -4055.83, 108.4676, "None"), Start=new Vector3(3395.09, -3380.25, 142.702, "None") },
            new Dungeon { Name="Stratholme - Service EntranceLoc",DungeonId =274, MapId=329, EntranceLoc=new Vector3(3237.718, -4055.83, 108.4676, "None"), Start=new Vector3(3591.732, -3643.578, 138.4914, "None") },
            new Dungeon { Name="The Deadmines",DungeonId =6, MapId=36, EntranceLoc=new Vector3(-11207.45, 1681.354, 23.80899, "None"), Start=new Vector3(-16.4, -383.07, 61.78, "None") },
            new Dungeon { Name="Stormwind Stockade",DungeonId =12, MapId=34, EntranceLoc=new Vector3(-8761.981, 847.7866, 86.25107, "None") },
            new Dungeon { Name="The Temple of Atal'Hakkar",DungeonId =28, MapId=109, EntranceLoc=new Vector3(-10169.46, -3997.184, -113.8935, "None")},
            new Dungeon { Name="Uldaman",DungeonId =22, MapId=70, EntranceLoc=new Vector3(-6059.759, -2955.001, 209.769, "None"), Start= new Vector3(-6059.759, -2955.001, 209.769, "None") },
            new Dungeon { Name="Uldaman Echomok Door, second Entrance", DungeonId=22, MapId=70, EntranceLoc=new Vector3(-6059.759, -2955.001, 209.769, "None"), Start= new Vector3(-212.3159, 383.2488, -38.72359, "None")},
            new Dungeon { Name="Wailing Caverns",DungeonId=1, MapId=43, EntranceLoc=new Vector3(-749.3314, -2212.979, 14.59487, "None") },
            new Dungeon { Name="Zul'Farrak",DungeonId =24, MapId=209, EntranceLoc=new Vector3(-6790.196, -2891.261, 8.902938, "None") },
            new Dungeon { Name="Auchenai Crypts",DungeonId =149, MapId=558, EntranceLoc=new Vector3(-3361.442, 5233.683, -101.0484, "None") },
            new Dungeon { Name="Hellfire Ramparts",DungeonId =136, MapId=543, EntranceLoc=new Vector3(-364.8607, 3083.738, -14.67881, "None") },
            new Dungeon { Name="Magisters' Terrace",DungeonId =198, MapId=585, EntranceLoc=new Vector3(12881.7, -7342.356, 65.52691, "None") },
            new Dungeon { Name="Mana Tombs",DungeonId =148, MapId=557, EntranceLoc=new Vector3(-3069.809, 4943.236, -101.0472, "None") },
            new Dungeon { Name="Old Hillsbrad Foothills",DungeonId =170, MapId=560, EntranceLoc=new Vector3(-8330.366, -4054.07, -207.6699, "None") },
            new Dungeon { Name="Sethekk Halls",DungeonId =150, MapId=556, EntranceLoc=new Vector3(-3361.055, 4652.112, -101.0487, "None") },
            new Dungeon { Name="Shadow Labyrinth",DungeonId =151, MapId=555, EntranceLoc=new Vector3(-3653.643, 4944.552, -101.3898, "None") },
            new Dungeon { Name="The Arcatraz",DungeonId =174, MapId=552, EntranceLoc=new Vector3(3313.221, 1328.768, 505.5585, "Flying") },
            new Dungeon { Name="The Black Morass",DungeonId =171, MapId=269, EntranceLoc=new Vector3(-8768.379, -4164.33, -210.2725, "None") },
            new Dungeon { Name="The Blood Furnace",DungeonId =137, MapId=542, EntranceLoc=new Vector3(-306.7688, 3168.562, 29.87161, "None") },
            new Dungeon { Name="The Botanica",DungeonId =173, MapId=553, EntranceLoc=new Vector3(3417.226, 1480.172, 182.8366, "Flying") },
            new Dungeon { Name="The Mechanar",DungeonId =172, MapId=554, EntranceLoc=new Vector3(2860.551, 1544.211, 252.159, "Flying") },
            new Dungeon { Name="The Shattered Halls",DungeonId =138, MapId=540, EntranceLoc=new Vector3(-310.0916, 3089.395, -4.094568, "None") },
            new Dungeon { Name="The Slave Pens",DungeonId =140, MapId=547, EntranceLoc=new Vector3(741.4343, 7011.924, -73.0747, "None") },
            new Dungeon { Name="The Steamvault",DungeonId =147, MapId=545, EntranceLoc=new Vector3(816.4943, 6952.726, -80.54607, "None") },
            new Dungeon { Name="The Underbog",DungeonId =146, MapId=546, EntranceLoc=new Vector3(783.4067, 6742.714, -72.53995, "None") },
            new Dungeon { Name="Ahn'kahet The Old Kingdom",DungeonId =218, MapId=619, EntranceLoc=new Vector3(3639.375, 2026.671, 2.541712, "None") },
            new Dungeon { Name="Azjol Nerub",DungeonId =204, MapId=601, EntranceLoc=new Vector3(3669.932, 2173.066, 36.05176, "None") },
            new Dungeon { Name="Drak'Tharon Keep",DungeonId =214, MapId=600, EntranceLoc=new Vector3(4774.752, -2018.308, 229.394, "None") },
            new Dungeon { Name="Gundrak",DungeonId =216, MapId=604, EntranceLoc=new Vector3(6972.771, -4399.461, 441.576, "None") },
            new Dungeon { Name="Halls of Lightning",DungeonId =207, MapId=602, EntranceLoc=new Vector3(9192.92, -1388.867, 1110.215, "Flying") },
            new Dungeon { Name="Halls of Reflection",DungeonId =255, MapId=668, EntranceLoc=new Vector3(0, 0,0, "None") },
            new Dungeon { Name="Halls of Stone",DungeonId =208, MapId=599, EntranceLoc=new Vector3(8920.499, -962.8192, 1039.132, "Flying") },
            new Dungeon { Name="Pit of Saron",DungeonId =253, MapId=658, EntranceLoc=new Vector3(0, 0,0, "None") },
            new Dungeon { Name="The Culling of Stratholme",DungeonId =209, MapId=595, EntranceLoc=new Vector3(-8757.122, -4466.833, -201.2474, "None") },
            new Dungeon { Name="The Forge of Souls",DungeonId =251, MapId=632, EntranceLoc=new Vector3(0, 0, 0, "None") },
            new Dungeon { Name="The Nexus",DungeonId =225, MapId=576, EntranceLoc=new Vector3(3906.412, 6985.285, 69.4881, "None") },
            new Dungeon { Name="The Oculus",DungeonId =206, MapId=578, EntranceLoc=new Vector3(3869.969, 6984.653, 108.1261, "None") },
            new Dungeon { Name="The Violet Hold",DungeonId =220, MapId=608, EntranceLoc=new Vector3(5675.277, 479.8194, 652.2078, "None") },
            new Dungeon { Name="Trial of the Champion",DungeonId =249, MapId=650, EntranceLoc=new Vector3(0, 0,0, "None") },
            new Dungeon { Name="Utgarde Keep",DungeonId =242, MapId=574, EntranceLoc=new Vector3(1236.232, -4859.632, 41.24857, "None") },
            new Dungeon { Name="Utgarde Pinnacle",DungeonId =203, MapId=575, EntranceLoc=new Vector3(1230.303, -4861.86, 218.289, "None") },
            new Dungeon{Name="The Forge of Souls",MapId = 632},
        };
    }
}
