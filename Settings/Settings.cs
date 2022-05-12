using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.IO;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.CrawlerSettings
{
    [Serializable]
    public class WholesomeDungeonCrawlerSettings : Settings
    {

        public string ProductName { get; set; }
        public string TankName { get; set; }
        public List<string> GroupMembers { get; set; }
        public bool SetAsTank { get; set; }
        public bool SetAsHeal { get; set; }
        public bool SetAsRDPS { get; set; }
        public bool SetAsMDPS { get; set; }
        public int FollowRangeRDPS { get; set; }
        public int FollowRangeMDPS { get; set; }
        public int FollowRangeHeal { get; set; }
        public WholesomeDungeonCrawlerSettings()
        {
            ProductName = "Wholesome Dungeon Crawler";
            TankName = "";
            GroupMembers = new List<string>();
            SetAsTank = false;
            SetAsHeal = false;
            SetAsMDPS = false;
            SetAsRDPS = false;
            FollowRangeHeal = 30;
            FollowRangeMDPS = 15;
            FollowRangeRDPS = 25;
        }

        public static WholesomeDungeonCrawlerSettings CurrentSetting { get; set; }
        public bool Save()
        {
            try
            {
                return Save(
                    AdviserFilePathAndName("WholesomeDungeonCrawlerSettings", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logger.LogError("WholesomeDungeonCrawlerSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("WholesomeDungeonCrawlerSettings",
                    ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<WholesomeDungeonCrawlerSettings>(AdviserFilePathAndName("WholesomeDungeonCrawlerSettings",
                            ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }

                CurrentSetting = new WholesomeDungeonCrawlerSettings();
            }
            catch (Exception e)
            {
                Logger.LogError("WholesomeDungeonCrawlerSettings > Load(): " + e);
            }

            return false;
        }
    }
}
