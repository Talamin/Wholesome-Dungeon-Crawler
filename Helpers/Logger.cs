using robotManager.Helpful;
using System.Drawing;
using Wholesome_Dungeon_Crawler.Bot;

namespace Wholesome_Dungeon_Crawler.Helpers
{
    class Logger
    {
        public static void LogError(string message)
        {
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Error, Color.DarkRed);
        }

        public static void Log(string message)
        {
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Normal, Color.DarkSlateBlue);
        }

        public static void LogDebug(string message)
        {
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Debug, Color.DarkGoldenrod);
        }
    }
}
