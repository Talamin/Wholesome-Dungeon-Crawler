using robotManager.FiniteStateMachine;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class GroupQueue : State, IState
    {
        public override string DisplayName => "Group Queue";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly int _selectedDungeonId;

        public GroupQueue(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _selectedDungeonId = WholesomeDungeonCrawlerSettings.CurrentSetting.SelectedDungeon;
        }

        public override bool NeedToRun
        {
            get
            {
                if (_entityCache.Me.Auras.Any(y => y.Key == 71041)
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
            if (!Lua.LuaDoString<bool>("return LFDQueueFrame:IsVisible()"))
            {
                Logger.Log($"Opening LFD frame");
                Lua.RunMacroText("/lfd");
            }
            else
            {
                Lua.LuaDoString("ResetInstances();");
                if (_selectedDungeonId > -1)
                {

                    if (!Lua.LuaDoString<bool>("return LFDQueueFrameSpecific:IsVisible()"))
                    {
                        Logger.Log($"Selecting dungeon {_selectedDungeonId} in dropdown list");
                        Lua.LuaDoString("LFDQueueFrameTypeDropDownButton:Click(); DropDownList1Button1:Click();");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        DungeonModel model = Lists.AllDungeons.Find(model => model.DungeonId == _selectedDungeonId);

                        if (model == null)
                        {
                            Logger.LogOnce($"Couldn't find matching dungeon model with ID {_selectedDungeonId}");
                            Thread.Sleep(5000);
                            return;
                        }

                        Logger.LogOnce($"Launching dungeon search for {model.Name}");
                        Lua.LuaDoString(@$"
                            for i=1, 1000, 1 do
                                LFDList_SetDungeonEnabled(i, false);
                            end
                            LFDList_SetDungeonEnabled({model.DungeonId}, true);
                            LFDQueueFrameSpecificList_Update();
                            LFDQueueFrameFindGroupButton:Click();
                        ");

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
                        Logger.LogOnce($"Launching random dungeon search");
                        Lua.LuaDoString("LFDQueueFrameFindGroupButton:Click()");
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
