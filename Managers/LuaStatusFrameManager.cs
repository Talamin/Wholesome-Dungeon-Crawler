using robotManager.Helpful;
using System.Diagnostics;
using System.Timers;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    class LuaStatusFrameManager : ILuaStatusFrameManager
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public static System.Timers.Timer LuaFrameUpdateTimer { get; set; }

        public LuaStatusFrameManager(ICache iCache, IEntityCache iEntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
        }

        public void Initialize()
        {
            CreateFrame();
            LuaFrameUpdateTimer = new System.Timers.Timer();
            LuaFrameUpdateTimer.Elapsed += LuaFrameUpdateTimer_Elapsed;
            LuaFrameUpdateTimer.Interval = 500;
            LuaFrameUpdateTimer.Start();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaWithArgs_OnEventsLuaStringWithArgs;
        }

        private void EventsLuaWithArgs_OnEventsLuaStringWithArgs(string eventid, System.Collections.Generic.List<string> args)
        {
            if (eventid == "CHAT_MSG_ADDON" && args[0] == "WDC" && args[1] == "Skip" && args[3] == _entityCache.Me.Name)
            {
                Debugger.Break();
                _profileManager.CurrentDungeonProfile.CurrentStep.MarkAsCompleted();
                Logger.Log("User skipped current step.");
            }
        }

        private void LuaFrameUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateFrame();
        }

        public void Dispose()
        {
            LuaFrameUpdateTimer.Dispose();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaWithArgs_OnEventsLuaStringWithArgs;
            HideFrame();
        }

        public void CreateFrame()
        {
            Lua.LuaDoString($@"
                        if not wdcrawler then
                            wdcrawler = CreateFrame(""Frame"",nil,UIParent)
                        end
                        wdcrawler:SetFrameStrata(""BACKGROUND"")
                        wdcrawler:SetWidth(250)
                        wdcrawler:SetHeight(160)
                        wdcrawler:SetBackdrop(StaticPopup1:GetBackdrop())
                        wdcrawler:SetBackdropBorderColor(0, 0, 0, 0)                            
                        wdcrawler:SetPoint(""RIGHT"", -100, 0)

                        if not wdcrawler.title then
                            wdcrawler.title = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.title:SetPoint(""TOP"", 0, -20)
                        wdcrawler.title:SetText(""WholesomeDCrawler Status"")
                        wdcrawler.title:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.title:SetFont(""Fonts\\ARIALN.TTF"", 15, ""OUTLINE"")
                        
                        if not wdcrawler.status then
                            wdcrawler.status = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.status:SetPoint(""TOPLEFT"", 20, -40)
                        wdcrawler.status:SetText(""Status:"")
                        wdcrawler.status:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.status:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.statustext then
                            wdcrawler.statustext = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.statustext:ClearAllPoints()
                        wdcrawler.statustext:SetPoint(""LEFT"",wdcrawler.status,""RIGHT"", 0,0)
                        wdcrawler.statustext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.statustext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.state then
                            wdcrawler.state = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.state:SetPoint(""TOPLEFT"", 20, -60)
                        wdcrawler.state:SetText(""State:"")
                        wdcrawler.state:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.state:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.statetext then
                            wdcrawler.statetext = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.statetext:ClearAllPoints()
                        wdcrawler.statetext:SetPoint(""LEFT"",wdcrawler.state,""RIGHT"", 0,0)
                        wdcrawler.statetext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.statetext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.following then
                            wdcrawler.following = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.following:SetPoint(""TOPLEFT"", 20, -80)
                        wdcrawler.following:SetText(""Following:"")
                        wdcrawler.following:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.following:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.followingtext then
                            wdcrawler.followingtext = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.followingtext:ClearAllPoints()
                        wdcrawler.followingtext:SetPoint(""LEFT"",wdcrawler.following,""RIGHT"", 0,0)
                        wdcrawler.followingtext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.followingtext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.dungeon then
                            wdcrawler.dungeon = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.dungeon:SetPoint(""TOPLEFT"", 20, -100)
                        wdcrawler.dungeon:SetText(""Dungeon:"")
                        wdcrawler.dungeon:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.dungeon:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.dungeontext then
                            wdcrawler.dungeontext = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.dungeontext:ClearAllPoints()
                        wdcrawler.dungeontext:SetPoint(""LEFT"",wdcrawler.dungeon,""RIGHT"", 0,0)
                        wdcrawler.dungeontext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.dungeontext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.step then
                            wdcrawler.step = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.step:SetPoint(""TOPLEFT"", 20, -120)
                        wdcrawler.step:SetText(""Step:"")
                        wdcrawler.step:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.step:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.steptext then
                            wdcrawler.steptext = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.steptext:ClearAllPoints()
                        wdcrawler.steptext:SetPoint(""LEFT"",wdcrawler.step,""RIGHT"", 0,0)
                        wdcrawler.steptext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.steptext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")


                        if not wdcrawler.stepskipbtn then
                            wdcrawler.stepskipbtn = CreateFrame(""BUTTON"", nil, wdcrawler)
                        end
                        wdcrawler.stepskipbtn:SetWidth(32)
                        wdcrawler.stepskipbtn:SetHeight(20)
                        wdcrawler.stepskipbtn:SetPoint(""LEFT"",wdcrawler.steptext,""RIGHT"", 0,0)
                        wdcrawler.stepskipbtn:SetBackdrop({{ edgeFile = ""Interface\\Buttons\\WHITE8X8"", edgeSize = 1 }})
                        wdcrawler.stepskipbtn:SetBackdropColor(.09,.09,.09,1)
	                    wdcrawler.stepskipbtn:SetBackdropBorderColor(.2,.2,.2,1)
                        wdcrawler.stepskipbtn:SetScript(""OnClick"", function(self, button, down) SendAddonMessage(""WDC"",""Skip"",""WHISPER"",""{_entityCache.Me.Name}"") end) 
                        wdcrawler.stepskipbtn.tooltiptext = ""Skips the current step.""

                        if not wdcrawler.stepskipbtn.stepskipbtntext then
                            wdcrawler.stepskipbtn.stepskipbtntext = wdcrawler.stepskipbtn:CreateFontString(nil, ""ARTWORK"", ""GameFontNormal"")
                        end
                        wdcrawler.stepskipbtn.stepskipbtntext:ClearAllPoints()
                        wdcrawler.stepskipbtn.stepskipbtntext:SetPoint(""LEFT"",wdcrawler.steptext,""RIGHT"", 3,0)
                        wdcrawler.stepskipbtn.stepskipbtntext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.stepskipbtn.stepskipbtntext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")
                        wdcrawler.stepskipbtn.stepskipbtntext:SetText(""Skip"")
                        wdcrawler.stepskipbtn.stepskipbtntext:SetTextColor(0.25, 0.9, 0.6, 1)

                        wdcrawler.stepskipbtn:SetFontString(wdcrawler.stepskipbtn.stepskipbtntext)

                        wdcrawler:SetMovable(true)
                        wdcrawler:EnableMouse(true)
                        wdcrawler:SetScript(""OnMouseDown"",function() wdcrawler:StartMoving() end)
                        wdcrawler:SetScript(""OnMouseUp"",function() wdcrawler:StopMovingOrSizing() end)

                        wdcrawler:Show();

                       ");
        }
        public void UpdateFrame()
        {
            var step = "N/A";
            var dung = "N/A";
            if (_profileManager.CurrentDungeonProfile != null)
            {
                step = _profileManager.CurrentDungeonProfile.CurrentStep != null ? _profileManager.CurrentDungeonProfile.CurrentStep.Name : "N/A";
                dung = _profileManager.CurrentDungeonProfile.MapId != 0 ? _profileManager.CurrentDungeonProfile.MapId.ToString() : "N/A";
            }
            var follow = _entityCache.TankUnit != null ? _entityCache.TankUnit.Name : "N/A";
            //var healer = _entityCache. != null ? Bot.lfgHealer.Name : "N/A";
            
            Lua.LuaDoString($@"
            if wdcrawler then
                wdcrawler.statustext:SetText(""{Logging.Status}"")
                wdcrawler.statetext:SetText(""{_cache.CurrentState}"")
                wdcrawler.followingtext:SetText(""{follow}"")
                wdcrawler.dungeontext:SetText(""{dung}"")
                wdcrawler.steptext:SetText(""{step}"")
            end
        ");
        }

        public void HideFrame()
        {
            Lua.LuaDoString($@"
            if wdcrawler then
                wdcrawler:Hide();
            end
        ");
        }
    }
}
