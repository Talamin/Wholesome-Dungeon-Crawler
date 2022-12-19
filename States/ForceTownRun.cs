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
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.Dead
                    || Fight.InFight
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep.Order > 0
                    || !_cache.IsInInstance
                    || _cache.LootRollShow)
                {
                    return false;
                }

                return ObjectManager.Me.GetDurabilityPercent < 30;
            }
        }


        public override void Run()
        {
            Logger.LogOnce($"We need a town run, leaving dungeon");
            MovementManager.StopMove();
            Thread.Sleep(1000);
            Lua.LuaDoString("LFGTeleport(true);");
            _cache.IsRunningForcedTownRun = true;
            Thread.Sleep(5000);
        }
    }
}
