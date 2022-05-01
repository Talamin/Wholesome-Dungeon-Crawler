using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Manager
{
    class ProfileManager : IProfileManager
    {
        private object profileLock = new object();
        public bool actualDungeonProfile { get; private set; }
        public Dungeon actualDungeon { get; private set; }

        private readonly IEntityCache _entityCache;

        public ProfileManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }
        public void Initialize()
        {
            CachePlayerEnteringWorld();
            //starting with Event Substcription
            EventsLua.AttachEventLua("PLAYER_ENTERING_WORLD", m => CachePlayerEnteringWorld());
            EventsLua.AttachEventLua("WORLD_MAP_UPDATE", m => CachePlayerEnteringWorld());
        }

        private void CachePlayerEnteringWorld()
        {   
            lock(profileLock)
            {
                actualDungeonProfile = Lists.AllDungeons.Any(d => d.MapId == Usefuls.ContinentId);
                if(actualDungeonProfile)
                {
                    actualDungeon = Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).OrderBy(o => o.Start.DistanceTo(_entityCache.Me.PositionWithoutType)).FirstOrDefault();
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
