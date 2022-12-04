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
    class LeaveAndReturnDungeon : State
    {
        public override string DisplayName => "Leaving and Returning to Dungeon";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public LeaveAndReturnDungeon(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
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
                    || !_entityCache.Me.Dead
                    || Fight.InFight
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep.Order > 0
                    || _cache.LootRollShow)
                {
                    return false;
                }

                return ObjectManager.Me.GetDurabilityPercent < 30 || !_cache.IsInInstance;
            }
        }


        public override void Run()
        {
            if (_cache.IsInInstance)
            {
                Logger.Log($"We need a town run, leaving dungeon");
                Lua.LuaDoString("LFGTeleport(true);");
                Thread.Sleep(5000);
            }
            else
            {
                Logger.Log($"Returning to dungeon");
                Lua.LuaDoString("LFGTeleport(false);");
                Thread.Sleep(5000);
            }
        }
    }
}
