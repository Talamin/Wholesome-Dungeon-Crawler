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
                    || !_entityCache.Me.Valid 
                    || Fight.InFight
                    || _entityCache.Me.Auras.Any(y => y.Key == 71041)
                    //|| ObjectManager.Me.HaveBuff("Dungeon Deserter") //71041
                    || _cache.IsInInstance
                    || _cache.ListPartyMemberNames.Count() < 4 //changed from 4 for testing
                    || !_cache.IAmTank)
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
