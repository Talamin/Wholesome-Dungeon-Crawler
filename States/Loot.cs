using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
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
        public Loot(ICache iCache, IEntityCache entityCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
            _entitycache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected 
                    || !ObjectManager.Me.IsValid 
                    || Fight.InFight
                    || Bag.GetContainerNumFreeSlots <= 2) // could be interesting to cache that too
                {
                    return false;
                }

                return _entitycache.EnemyUnitsLootable.Count() > 0;
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
            Vector3 myPosition = ObjectManager.Me.Position;
            return ObjectManager.GetWoWUnitLootable()
                .FindAll(u => u.Position.DistanceTo(myPosition) <= LootRange)
                .ToList();
        }
    }
}
