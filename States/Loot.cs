using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class Loot : State, IState
    {
        public override string DisplayName => "Looting";
        private readonly int LootRange = 20;

        private readonly ICache _cache;
        private readonly IEntityCache _entitycache;
        public Loot(ICache iCache, IEntityCache entityCache)
        {
            _cache = iCache;
            _entitycache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entitycache.Me.Valid
                    || Fight.InFight
                    || Bag.GetContainerNumFreeSlots <= 2) 
                {
                    return false;
                }

                return GetListLootableUnits().Count() > 0;
            }
        }


        public override void Run()
        {
            LootingTask.Pulse(GetListLootableUnits());
            Lua.LuaDoString("CloseLoot()");
        }

        // should use cache instead of this method
        private List<WoWUnit> GetListLootableUnits()
        {
            Vector3 myPosition = _entitycache.Me.PositionWithoutType;
            return ObjectManager.GetWoWUnitLootable()
                .FindAll(u => u.Position.DistanceTo(myPosition) <= LootRange)
                .ToList();
        }
    }
}
