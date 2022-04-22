using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class GroupQueue : State, IState
    {
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
                if (!Conditions.InGameAndConnected || !ObjectManager.Me.IsValid || Fight.InFight)
                {
                    return false;
                }
                if (!_cache.IsInInstance)
                {
                    return false;
                }
                if(_cache.ListPartyMember.Count() < 5)
                {
                    return false;
                }
                if(_cache.NotInLFG && !ObjectManager.Me.HaveBuff("Dungeon Deserter"))
                {
                    return true;
                }

                return false;
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
