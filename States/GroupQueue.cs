using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class GroupQueue : State, IState
    {
        public override string DisplayName => "GroupQueue";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public GroupQueue(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || _entityCache.Me.Auras.Any(y => y.Key == 71041)
                    || _cache.IsInInstance
                    || _entityCache.ListPartyMemberNames.Count() < 4 //changed from 4 for testing
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                //return Lua.LuaDoString<bool>("local mode, submode = GetLFGMode(); return mode == nil;");
                return !Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon:IsVisible()");
            }
        }

        public override void Run()
        {
            if (!Lua.LuaDoString<bool>("return LFDQueueFrame:IsVisible()"))
            {
                Logger.Log($"Opening LFD frame");
                Lua.RunMacroText("/lfd");
            }
            else
            {
                if (!Lua.LuaDoString<bool>("return LFDQueueFrameRandom:IsVisible()"))
                {
                    Logger.Log($"Selecting random in dropdown list");
                    Lua.LuaDoString("LFDQueueFrameTypeDropDownButton:Click(); DropDownList1Button2:Click()");
                }
                else
                {
                    Logger.Log($"Launching dungeon search");
                    Lua.LuaDoString("LFDQueueFrameFindGroupButton:Click()");
                }
            }
        }
    }
}
