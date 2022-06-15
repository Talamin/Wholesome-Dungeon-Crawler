using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class LeaveDungeon : State
    {
        public override string DisplayName => "Leaving Dungeon";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public LeaveDungeon(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
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
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || _cache.LootRollShow)
                {
                    return false;
                }

                return _profileManager.CurrentDungeonProfile.ProfileIsCompleted;
            }
        }


        public override void Run()
        {
            Logger.Log($"Leaving dungeon in 10 seconds.");
            Thread.Sleep(10 * 1000);
            //Lua.LuaDoString("LFGTeleport(true);");
            Toolbox.LeaveDungeonAndGroup();
        }
    }
}
