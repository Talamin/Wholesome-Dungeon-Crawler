using robotManager.FiniteStateMachine;
using System.Linq;
using System.Threading;
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
                    || !_entityCache.Me.IsValid
                    || Fight.InFight
                    || _entityCache.Me.Auras.Any(y => y.Key == 71041)
                    || _cache.IsInInstance
                    || _entityCache.ListPartyMemberNames.Count() < 4
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                return !Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon:IsVisible()");
            }
        }

        public override void Run()
        {
            int specificDungeonId = -1;

            if (!Lua.LuaDoString<bool>("return LFDQueueFrame:IsVisible()"))
            {
                Logger.Log($"Opening LFD frame");
                Lua.RunMacroText("/lfd");
            }
            else
            {
                if (specificDungeonId > -1)
                {
                    if (!Lua.LuaDoString<bool>("return LFDQueueFrameSpecific:IsVisible()"))
                    {
                        Logger.Log($"Selecting dungeon {specificDungeonId} in dropdown list");
                        Lua.LuaDoString("LFDQueueFrameTypeDropDownButton:Click(); DropDownList1Button1:Click()");
                    }
                    else
                    {
                        Lua.LuaDoString($@"
                            for i=1,30,1 do
                                local button = _G[""LFDQueueFrameSpecificListButton"" .. i .. ""EnableButton""];
                                if button ~= nil then
                                    -- Select if wanted dungeon
                                    local dungeonId = _G[""LFDQueueFrameSpecificListButton"" .. i].id;
                                    if dungeonId ~= nil then
                                        if LFGIsIDHeader(dungeonId) == false then
                                            if dungeonId == {specificDungeonId} then
                                                if button:GetChecked() ~= 1 then
                                                    button:Click();
                                                end
                                            -- Unselect the rest
                                            elseif button:GetChecked() == 1 then
                                                button:Click();
                                            end
                                        end
                                    end
                                end
                            end
                        ");
                        Logger.LogOnce($"Launching dungeon search");
                        Lua.LuaDoString("LFDQueueFrameFindGroupButton:Click()");
                        Thread.Sleep(1000);
                    }
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
                        Logger.LogOnce($"Launching dungeon search");
                        Lua.LuaDoString("LFDQueueFrameFindGroupButton:Click()");
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
