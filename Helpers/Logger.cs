using robotManager.Helpful;
using System.Drawing;

namespace WholesomeDungeonCrawler.Helpers
{
    static class Logger
    {
        private static string _lastMessage;

        public static void LogError(string message)
        {
            Logging.Write($"[WDC]: {message}", Logging.LogType.Error, Color.DarkRed);
        }

        public static void Log(string message)
        {
            Logging.Write($"[WDC]: {message}", Logging.LogType.Normal, Color.DarkSlateBlue);
            //Logging.Status = message;
        }

        public static void LogDebug(string message)
        {
            Logging.Write($"[WDC]: {message}", Logging.LogType.Debug, Color.DarkGoldenrod);
        }

        public static void LogOnce(string message, bool error = false)
        {
            if (message != _lastMessage)
            {
                if (error)
                    LogError(message);
                else
                    Log(message);
                _lastMessage = message;
            }
        }
    }
}
