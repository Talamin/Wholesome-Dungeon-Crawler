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

        public GroupQueue(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected 
                    || !ObjectManager.Me.IsValid 
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _cache.ListPartyMember.Count() < 5)
                {
                    return false;
                }

                return _cache.GetLFGMode == "nil" && !ObjectManager.Me.HaveBuff("Dungeon Deserter");
            }
        }

        public override void Run()
        {
            Logger.Log("Queuing for Dungens!");

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
