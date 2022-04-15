using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class Cache
    {

        public static bool _IsInInstance { get; private set; }

        private static object _cacheLock = new object();

        public Cache()
        {
        }

        public void Initialize()
        {
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            EventsLua.AttachEventLua("WORLD_MAP_UPDATE", m => cacheOnEvents());
        }


        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }


        private void OnObjectManagerPulse()
        {
            lock (_cacheLock)
            {
                
            }

        }

        private void cacheOnEvents()
        {
            lock (_cacheLock)
            {
                _IsInInstance = WTLocation.IsInInstance();
            }

        }

    }
}
