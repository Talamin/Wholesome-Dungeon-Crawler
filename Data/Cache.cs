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

        internal bool isInInstance { get; private set; }

        internal object _cacheLock = new object();

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
                isInInstance = WTLocation.IsInInstance();
            }

        }

    }
}
