using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    public class Cache
    {
        public static bool Initialized { get; private set; }

        public static bool cIsInInstance { get; private set; }

        private static object _cacheLock = new object();

        public static void Initialize()
        {
            lock(_cacheLock)
            {
                cacheGeneral();
                Initialized = true;
            }
        }

        public static void Dispose()
        {
            lock (_cacheLock)
            {
                Initialized = false;
            }
        }

        private static void cacheGeneral()
        {
            lock(_cacheLock)
            {
                cIsInInstance = WTLocation.IsInInstance();
            }
            
        }

    }
}
