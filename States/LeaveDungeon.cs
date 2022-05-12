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
        private IWoWUnit Target;

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

                return _profileManager.CurrentDungeonProfile._profileSteps.Count(p=> p.IsCompleted) >= _profileManager.CurrentDungeonProfile._profileSteps.Count();
            }
        }


        public override void Run()
        {
            Logger.Log($"All Steps completed, leaving  Dungeon, waiting some seconds for finishing Reward Roll");
            Thread.Sleep(5000);
            Lua.LuaDoString("LFGTeleport(true);");
        }

    }
}
