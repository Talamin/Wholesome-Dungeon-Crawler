using robotManager.Helpful;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;

namespace WholesomeDungeonCrawler.Helpers
{
    class Logger
    {
        public static void LogError(string message)
        {
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Error, Color.DarkRed);
        }

        public static void Log(string message)
        {            
            var lastmsg = Logging.ReadList(Logging.LogType.Normal);
            if (lastmsg.Where(t=>System.DateTime.Now - t.DateTime < new System.TimeSpan(0,0,5)).Any(x=>x.Text == message)) return;
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Normal, Color.DarkSlateBlue);
            Logging.Status = message;
        }

        public static void LogDebug(string message)
        {
            Logging.Write($"[{WholesomeDungeonCrawlerSettings.CurrentSetting.ProductName}]: {message}", Logging.LogType.Debug, Color.DarkGoldenrod);
        }
    }
}
