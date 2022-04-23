﻿using robotManager.FiniteStateMachine;
using Timer = robotManager.Helpful.Timer;
using System.Collections.Generic;
using System.Linq;
using wManager.Wow.ObjectManager;
using wManager.Wow.Helpers;
using robotManager.Helpful;
using wManager.Wow.Bot.Tasks;
using WholesomeDungeonCrawler.Data;

namespace WholesomeDungeonCrawler.States
{
    public class Loot : State, IState
    {
        public override string DisplayName
        {
            get { return "Looting"; }
        }
        public Loot(int priority)
        {
            Priority = priority;
        }

        private  int LootRange = 20;
        public WoWUnit LootUnit;
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected || !ObjectManager.Me.IsValid || Fight.InFight)
                {
                    return false;
                }

                if (Bag.GetContainerNumFreeSlots <= 2)
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