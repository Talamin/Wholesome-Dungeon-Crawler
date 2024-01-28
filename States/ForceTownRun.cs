using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class ForceTownRun : State
    {
        public override string DisplayName => "Leaving dungeon for a town run";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public ForceTownRun(ICache iCache, 
            IEntityCache EntityCache, 
            IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_profileManager.ProfileIsRunning
                    || _profileManager.CurrentDungeonProfile.GetCurrentStepIndex > 0
                    || !_cache.IsInInstance)
                {
                    return false;
                }

                return ObjectManager.Me.GetDurabilityPercent < 30;
            }
        }

        public override void Run()
        {
            Logger.LogOnce($"Durability under 30%. We need to repair, leaving dungeon");
            MovementManager.StopMove();
            Thread.Sleep(1000);
            Lua.LuaDoString("LFGTeleport(true);");
            _cache.IsRunningForcedTownRun = true;
            Thread.Sleep(5000);
        }
    }
}
