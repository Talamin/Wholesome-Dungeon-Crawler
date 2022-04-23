using robotManager.Helpful;
using System;
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

        public WholesomeDungeonCrawlerSettings()
        {
            ProductName = "Wholesome Dungeon Crawler";
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
