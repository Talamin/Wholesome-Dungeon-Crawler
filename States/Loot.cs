using robotManager.FiniteStateMachine;
using System;
using Timer = robotManager.Helpful.Timer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wManager.Wow.ObjectManager;
using wManager.Wow.Helpers;
using robotManager.Helpful;
using wManager.Wow.Bot.Tasks;

namespace WholesomeDungeonCrawler.States
{
    public class Loot : State
    {
        public override string DisplayName
        {
            get { return "Looting"; }
        }

        private  int LootRange = 20;
        public static Timer ResetLootBlacklist = new Timer();
        public WoWUnit LootUnit;
        public override bool NeedToRun
        {
            get
            {
                if(!Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight)

                if(Bag.GetContainerNumFreeSlots <= 2)
                    return false;

                if (GetListLootableUnits().Count > 0)
                    return true;

                return false;
            }
        }

        public override void Run()
        {
            LootingTask.Pulse(GetListLootableUnits());
            Lua.LuaDoString("CloseLoot()");
        }

        private List<WoWUnit> GetListLootableUnits()
        {
            Vector3 myPosition = ObjectManager.Me.Position;
            return ObjectManager.GetWoWUnitLootable()
                .FindAll(u => u.Position.DistanceTo(myPosition) <= LootRange)
                .ToList();
        }

    }
}
