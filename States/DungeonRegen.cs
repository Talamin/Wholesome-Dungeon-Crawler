using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class DungeonRegen : State, IState
    {
        public override string DisplayName => "Dungeon Regen";

        private readonly IEntityCache _entityCache;
        private string _selectedFood;
        private string _selectedDrink;

        public DungeonRegen(IEntityCache iEntityCache)
        {
            _entityCache = iEntityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (_entityCache.Me.Swimming
                    || ObjectManager.Me.IsCast
                    || _entityCache.EnemiesAttackingGroup.Length > 0)
                {
                    return false;
                }

                // Check food
                if (_entityCache.Me.HealthPercent < wManagerSetting.CurrentSetting.FoodPercent)
                {
                    // Set food
                    List<WoWItem> bagItems = Bag.GetBagItem();
                    foreach (WoWItem item in bagItems.OrderByDescending(item => item.GetItemInfo.ItemMinLevel))
                    {
                        if (item.GetItemInfo.ItemMinLevel <= ObjectManager.Me.Level
                            && _allFoods.Contains(item.Name))
                        {
                            _selectedFood = item.Name;
                            break;
                        }
                    }
                    return true;
                }

                // Check drink
                if (_entityCache.Me.Mana > 0
                    && wManagerSetting.CurrentSetting.RestingMana
                    && _entityCache.Me.ManaPercent < wManagerSetting.CurrentSetting.DrinkPercent)
                {
                    // Set drink
                    List<WoWItem> bagItems = Bag.GetBagItem();
                    foreach (WoWItem item in bagItems.OrderByDescending(item => item.GetItemInfo.ItemMinLevel))
                    {
                        if (item.GetItemInfo.ItemMinLevel <= ObjectManager.Me.Level
                            && _allDrinks.Contains(item.Name))
                        {
                            _selectedDrink = item.Name;
                            break;
                        }
                    }
                    return true;
                }

                // Drinking/Eating
                if (_entityCache.Me.HasDrinkBuff
                    || _entityCache.Me.HasFoodBuff)
                {
                    return true;
                }


                return false;
            }
        }

        public override void Run()
        {
            Thread.Sleep(500);
            MovementManager.StopMove();

            // We get stats from LUA because wrobot returns obsolete health/mana info
            // Note: This is why sometimes wrobot keeps drinking despite being full mana
            int[] realStats = Lua.LuaDoString<int[]>($@"
                local result = {{}};
                table.insert(result, UnitHealth('player'));
                table.insert(result, UnitHealthMax('player'));
                table.insert(result, UnitPower('player', 0));
                table.insert(result, UnitPowerMax('player', 0));
                return unpack(result)
            ");

            int currentHealth = realStats[0];
            int maxHealth = realStats[1];
            int currentMana = realStats[2];
            int maxMana = realStats[3];
            int currentHealthPercent = currentHealth / maxHealth * 100;
            int currentManaPercent = currentMana / maxMana * 100;

            // Currently eating
            if (_entityCache.Me.HasFoodBuff
                && currentHealthPercent >= 99)
            {
                Lua.LuaDoString($"CancelUnitBuff('player', 'Food');");
                return;
            }

            // Currently drinking
            if (_entityCache.Me.HasDrinkBuff
                && currentManaPercent >= 99)
            {
                Lua.LuaDoString($"CancelUnitBuff('player', 'Drink');");
                return;
            }

            if (currentHealthPercent < wManagerSetting.CurrentSetting.FoodPercent
                && !_entityCache.Me.HasFoodBuff
                && !string.IsNullOrEmpty(_selectedFood))
            {
                Logger.LogOnce($"Eating {_selectedFood} ({ItemsManager.GetItemCountByNameLUA(_selectedFood)} in bags)");
                ItemsManager.UseItem(_selectedFood);
                Thread.Sleep(1000);
                return;
            }

            if (currentManaPercent < wManagerSetting.CurrentSetting.DrinkPercent
                && !_entityCache.Me.HasDrinkBuff
                && !string.IsNullOrEmpty(_selectedDrink))
            {
                Logger.LogOnce($"Drinking {_selectedDrink} ({ItemsManager.GetItemCountByNameLUA(_selectedDrink)} in bags)");
                ItemsManager.UseItem(_selectedDrink);
                Thread.Sleep(1000);
                return;
            }
        }


        private readonly List<string> _allDrinks = new List<string>()
        {
            "Honeymint Tea", "Yeti Milk", "Crusader's Waterskin", "Pungent Seal Whey", "Bitter Plasma",
            "Fresh Apple Juice", "Fresh-Squeezed Limeade", "Purified Draenic Water", "Ethermead",
            "Sparkling Southshore Cider", "Sweetened Goat's Milk", "Frostberry Juice", "Grizzleberry Juice",
            "Filtered Draenic Water", "Silverwine", "Morning Glory Dew", "Moonberry Juice", "Bottled Winterspring Water",
            "Sweet Nectar", "Melon Juice", "Fizzy Faire Drink", "Ice Cold Milk", "Blended Bean Brew",
            "Refreshing Spring Water", "Conjured Mana Strudel", "Conjured Mana Biscuit", "Conjured Mana Pie",
            "Conjured Glacier Water", "Conjured Fresh Water", "Conjured Water", "Conjured Sparkling Water",
            "Conjured Crystal Water", "Conjured Mountain Spring Water", "Conjured Purified Water",
            "Conjured Spring Water", "Conjured Mineral Water"
        };

        private readonly List<string> _allFoods = new List<string>()
        {
            "Sparkling Frostcap", "Savory Snowplum", "Sweet Potato Bread", "Poached Emperor Salmon",
            "Briny Hardcheese", "Mead Basted Caribou", "Sizzling Grizzly Flank", "Stewed Drakeflesh",
            "Steaming Chicken Soup", "Lyribread", "Mag'har Mild Cheese", "Bladespire Bagel", "Telaari Grapes",
            "Clefthoof Ribs", "Zangar Trout", "Sporeggar Mushroom", "Sour Goat Cheese", "Crusty Flatbread",
            "Fillet of Icefin", "Honey-Spiced Lichen", "Salted Venison", "Tundra Berries", "Frostberries",
            "Grizzleberries", "Raw Tallhorn Chunk", "Fresh Eagle Meat", "Dirge's Kickin' Chimaerok Chops",
            "Marsh Lichen", "Smoked Talbuk Venison", "Mag'har Grainbread", "Skethyl Berries", "Garadar Sharp",
            "Sunspring Carp", "Zangar Caps", "Moser's Magnificent Muffin", "Diamond Berries", "Alterac Swiss",
            "Dried King Bolete", "Homemade Cherry Pie", "Roasted Quail", "Deep Fried Plantains", "Spinefin Halibut",
            "Grim Guzzler Boar", "Lobster Stew", "Cabbage Kimchi", "Radish Kimchi", "Friendship Bread",
            "Fine Aged Cheddar", "Cured Ham Steak", "Soft Banana Bread", "Moon Harvest Pumpkin", "Raw Black Truffle",
            "Grilled King Crawler Legs", "Winter Squid", "Grilled Squid", "Filet of Redgill", "Nightfin Soup",
            "Heaven Peach", "Crunchy Frog", "Darnassus Kimchi Pie", "Striped Yellowtail", "Stormwind Brie",
            "Wild Hog Shank", "Goldenbark Apple", "Mulgore Spice Bread", "Rockscale Cod", "Delicious Cave Mold",
            "Bloodbelly Fish", "Wild Ricecake", "Red Hot Wings", "Dwarven Mild", "Mutton Chop", "Snapvine Watermelon",
            "Moist Cornbread", "Bristle Whisker Catfish", "Spongy Morel", "Pickled Kodo Foot", "Dalaran Sharp",
            "Goretusk Liver Pie", "Haunch of Meat", "Tel'Abim Banana", "Freshly Baked Bread", "Longjaw Mud Snapper",
            "Red-speckled Mushroom", "Loch Frenzy Delight", "Smoked Bear Meat", "Deeprun Rat Kabob", "Holiday Cheesewheel",
            "Spiced Beef Jerky", "Tough Jerky", "Slitherskin Mackerel", "Darnassian Bleu", "Shiny Red Apple",
            "Tough Hunk of Bread", "Forest Mushroom Cap", "Small Pumpkin", "Ripe Watermelon", "Darkmoon Dog",
            "Conjured Mana Pie", "Conjured Muffin", "Conjured Cinnamon Roll", "Conjured Bread", "Conjured Sourdough",
            "Conjured Croissant", "Conjured Rye", "Conjured Pumpernickel", "Conjured Sweet Roll"
        };
    }
}
