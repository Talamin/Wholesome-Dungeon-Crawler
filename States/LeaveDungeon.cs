using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class LeaveDungeon:  State
    {
        public override string DisplayName => "Leaving Dungeon";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public LeaveDungeon(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null) // could be interesting to cache that too
                {
                    return false;
                }

                if(_cache.LootRollShow)
                {
                    return false;
                }

                return _profileManager.CurrentDungeonProfile._profileSteps.All(p=> p.IsCompleted);
            }
        }


        public override void Run()
        {           
            Lua.LuaDoString("LFGTeleport(true);");
        }

    }
}
