using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class Cache
    {

        public static bool cIsInInstance { get; private set; }

        private static object _cacheLock = new object();

        public static void Initialize() => ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;

        public static void Dispose() => ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;


        private static void OnObjectManagerPulse()
        {
            lock (_cacheLock)
            {
                cacheGeneral();
            }

        }

        private static void cacheGeneral()
        {
            lock (_cacheLock)
            {
                cIsInInstance = WTLocation.IsInInstance();
            }

        }

    }
}
