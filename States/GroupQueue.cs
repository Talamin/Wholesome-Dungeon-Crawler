using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class GroupQueue : State, IState
    {
        public override string DisplayName => "GroupQueue";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private string tankname = "Tank";

        public GroupQueue(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected 
                    || !ObjectManager.Me.IsValid 
                    || Fight.InFight
                    || ObjectManager.Me.HaveBuff("Dungeon Deserter")
                    || _cache.IsInInstance
                    || _cache.ListPartyMember.Count() < 4 //changed from 4 for testing
                    || _entityCache.Me.Name != tankname)
                {
                    return false;
                }

                return Lua.LuaDoString<string>("mode, submode= GetLFGMode(); if mode == nil then return 'nil' else return mode end;") == "nil";
            }
        }

        public override void Run()
        {
            if (!Lua.LuaDoString<bool>("return LFDQueueFrame: IsVisible()"))
            {
                Lua.RunMacroText("/lfd");
            }

            if (Lua.LuaDoString<bool>("return LFDQueueFrame: IsVisible()") && !Lua.LuaDoString<bool>("return LFDQueueFrameRandom: IsVisible()"))
            {
                Lua.LuaDoString("LFDQueueFrameTypeDropDownButton:Click(); DropDownList1Button2:Click()");
            }

            if (Lua.LuaDoString<bool>("return LFDQueueFrame: IsVisible()") && Lua.LuaDoString<bool>("return LFDQueueFrameRandom: IsVisible()"))
            {
                Lua.LuaDoString("LFDQueueFrameFindGroupButton:Click()");
            }
        }
    }
}
