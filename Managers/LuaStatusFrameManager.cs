
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Timers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    class LuaStatusFrameManager : ILuaStatusFrameManager
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private string _loggingStatus = null;
        private string _currentState = null;
        private string _tankName = null;
        private string _dungeonName = null;
        private static readonly int _defaultFrameHeight = 160;
        // Mem
        private string _lastStepName;
        private bool _lastStepCompletion;
        private int _lastNumberOfSteps = 0;

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
        }

        private void LuaFrameUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateFrame();
        }

        public void Dispose()
        {
            LuaFrameUpdateTimer.Dispose();
            HideFrame();
        }

        public void CreateFrame()
        {
            // Timer frame
            Lua.LuaDoString($@"
                        if not wdcrawlerTimer then
                            wdcrawlerTimer = CreateFrame(""Frame"",nil,UIParent)
                        end
                        wdcrawlerTimer:SetFrameStrata(""BACKGROUND"")
                        wdcrawlerTimer:SetWidth(250)
                        wdcrawlerTimer:SetHeight(100)
                        wdcrawlerTimer:SetBackdrop(StaticPopup1:GetBackdrop())
                        wdcrawlerTimer:SetBackdropBorderColor(0.2, 0.2, 0.2, 0.7)                            
                        wdcrawlerTimer:SetPoint(""CENTER"", 0, 150)
                        
                        if not wdcrawlerTimer.defTimerText then
                            wdcrawlerTimer.defTimerText = wdcrawlerTimer:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                            wdcrawlerTimer.defTimerText:SetText(""Defend spot timer : --:--"")
                            wdcrawlerTimer.defTimerText:SetTextColor(0.4, 0.4, 0.4)
                        end
                        wdcrawlerTimer.defTimerText:SetPoint(""TOP"", 0, -25)
                        wdcrawlerTimer.defTimerText:SetFont(""Fonts\\ARIALN.TTF"", 16, ""OUTLINE"")
                        
                        if not wdcrawlerTimer.condTimerText then
                            wdcrawlerTimer.condTimerText = wdcrawlerTimer:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                            wdcrawlerTimer.condTimerText:SetText(""Condition timer : --:--"")
                            wdcrawlerTimer.condTimerText:SetTextColor(0.4, 0.4, 0.4)
                        end
                        wdcrawlerTimer.condTimerText:SetPoint(""TOP"", 0, -55)
                        wdcrawlerTimer.condTimerText:SetFont(""Fonts\\ARIALN.TTF"", 16, ""OUTLINE"")

                        wdcrawlerTimer:Hide();
                ");

            // Main Crawler frame
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
                        wdcrawler.following:SetText(""Tank:"")
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

                        if not wdcrawler.allsteps then
                            wdcrawler.allsteps = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                        end
                        wdcrawler.allsteps:SetPoint(""TOPLEFT"", 20, -120)
                        wdcrawler.allsteps:SetText(""Steps:"")
                        wdcrawler.allsteps:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.allsteps:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")

                        if not wdcrawler.stepskipbtn then
                            wdcrawler.stepskipbtn = CreateFrame(""BUTTON"", nil, wdcrawler)
                        end
                        wdcrawler.stepskipbtn:SetWidth(32)
                        wdcrawler.stepskipbtn:SetHeight(20)
                        wdcrawler.stepskipbtn:SetPoint(""LEFT"",wdcrawler.allsteps,""RIGHT"", 0,0)
                        wdcrawler.stepskipbtn:SetBackdrop({{ edgeFile = ""Interface\\Buttons\\WHITE8X8"", edgeSize = 1 }})
                        wdcrawler.stepskipbtn:SetBackdropColor(.09,.09,.09,1)
	                    wdcrawler.stepskipbtn:SetBackdropBorderColor(.2,.2,.2,1)                        
                        wdcrawler.stepskipbtn.tooltiptext = ""Skips the current step.""
                        wdcrawler.stepskipbtn:Hide()

                        if not wdcrawler.stepskipbtn.stepskipbtntext then
                            wdcrawler.stepskipbtn.stepskipbtntext = wdcrawler.stepskipbtn:CreateFontString(nil, ""ARTWORK"", ""GameFontNormal"")
                        end
                        wdcrawler.stepskipbtn.stepskipbtntext:ClearAllPoints()
                        wdcrawler.stepskipbtn.stepskipbtntext:SetPoint(""LEFT"",wdcrawler.stepskipbtn,""LEFT"", 3,0)
                        wdcrawler.stepskipbtn.stepskipbtntext:SetTextColor(1, 1, 1, 1)
                        wdcrawler.stepskipbtn.stepskipbtntext:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")
                        wdcrawler.stepskipbtn.stepskipbtntext:SetText(""Skip"")
                        wdcrawler.stepskipbtn.stepskipbtntext:SetTextColor(0.25, 0.9, 0.6, 1)
                        wdcrawler.stepskipbtn.stepskipbtntext:Hide()

                        wdcrawler.stepskipbtn:SetFontString(wdcrawler.stepskipbtn.stepskipbtntext)

                        wdcrawler:SetMovable(true)
                        wdcrawler:EnableMouse(true)
                        wdcrawler:SetScript(""OnMouseDown"",function() wdcrawler:StartMoving() end)
                        wdcrawler:SetScript(""OnMouseUp"",function() wdcrawler:StopMovingOrSizing() end)

                        wdcrawler:Show();");
        }

        public void UpdateFrame()
        {
            try
            {
                string allLua = "";

                // Dungeon Name
                string dungeonName = _profileManager.CurrentDungeonProfile != null ? _profileManager.CurrentDungeonProfile.FileName : "N/A";
                if (dungeonName != _dungeonName)
                {
                    allLua += @$"
                    wdcrawler.dungeontext:SetText(""{dungeonName}"")";
                    _dungeonName = dungeonName;
                }

                // Tank Name
                string tankName = _entityCache.TankUnit != null ? _entityCache.TankUnit.Name : "N/A";
                if (tankName != _tankName)
                {
                    allLua += @$"
                    wdcrawler.followingtext:SetText(""{tankName}"")";
                    _tankName = tankName;
                }

                // Status
                string loggingStatus = Logging.Status;
                if (_loggingStatus != loggingStatus)
                {
                    allLua += @$"
                    wdcrawler.statustext:SetText(""{loggingStatus}"")";
                    _loggingStatus = loggingStatus;
                }

                // State
                string currentState = _cache.CurrentState;
                if (currentState != _currentState)
                {
                    allLua += @$"
                    wdcrawler.statetext:SetText(""{_cache.CurrentState}"")";
                    _currentState = currentState;
                }

                int currentNumberOfSteps;

                if (_profileManager.CurrentDungeonProfile != null
                    && _profileManager.CurrentDungeonProfile.CurrentStep != null)
                {
                    List<IStep> allSteps = _profileManager.CurrentDungeonProfile.GetAllSteps;
                    currentNumberOfSteps = allSteps.Count;
                    int stepsBasePosition = -140;
                    IStep currentStep = _profileManager.CurrentDungeonProfile.CurrentStep;

                    // Detect change in the step list
                    if (currentNumberOfSteps != _lastNumberOfSteps
                        || _lastStepCompletion != currentStep.IsCompleted
                        || _lastStepName != currentStep.Name)
                    {
                        // Delete all previous steps
                        for (int i = 0; i < 100; i++)
                        {
                            allLua += $@"
                                if wdcrawler.allstepsText{i} then
                                    wdcrawler.allstepsText{i}:SetText("""")
                                end
                                ";
                        }

                        allLua += $@"
                                wdcrawler.allsteps:SetText(""Steps ({allSteps.Count}):"")
                            ";

                        // Steps autoscroll
                        int currentStepIndex = allSteps.IndexOf(currentStep);
                        int size = 4;
                        int indexStart = currentStepIndex - size;
                        int startOffset = indexStart < 0 ? 0 - indexStart : 0;
                        int range = System.Math.Min(size * 2 + 1, currentNumberOfSteps);
                        List<IStep> stepsToDisplay = new List<IStep>(allSteps.GetRange(indexStart + startOffset, range));
                        int stepsToDisplayLength = stepsToDisplay.Count;

                        for (int i = 0; i < stepsToDisplayLength; i++)
                        {
                            // step text color
                            string color = "1, 1, 1, 1";
                            if (stepsToDisplay[i].IsCompleted) color = "0.5, 0.5, 0.5";
                            if (stepsToDisplay[i] == currentStep)
                            {
                                color = "0.5, 0.5, 1";
                            }

                            allLua += $@"
                            if not wdcrawler.allstepsText{i} then
                                wdcrawler.allstepsText{i} = wdcrawler:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
                                wdcrawler.allstepsText{i}:ClearAllPoints()
                                wdcrawler.allstepsText{i}:SetPoint(""TOPLEFT"",60,{stepsBasePosition})
                                wdcrawler.allstepsText{i}:SetFont(""Fonts\\ARIALN.TTF"", 12, ""OUTLINE"")
                            end
                            if not wdcrawler.allstepsText{i}:IsVisible() then
                                wdcrawler.allstepsText{i}:Show()                            
                            end
                            wdcrawler.allstepsText{i}:SetText(""{allSteps.IndexOf(stepsToDisplay[i]) + 1}. {stepsToDisplay[i].Name}"")
                            wdcrawler.allstepsText{i}:SetTextColor({color})";

                            stepsBasePosition -= 17;
                        }

                        // Frame height
                        allLua += @$"
                            wdcrawler:SetHeight({_defaultFrameHeight + 17 * stepsToDisplayLength})";
                    }

                    // Timer text
                    bool shouldShowTimer = false;
                    if (currentStep != null)
                    {
                        // Defend spot timer
                        if (currentStep is DefendSpotStep defendSpotStep)
                        {
                            shouldShowTimer = true;
                            double timeLeft = defendSpotStep.GetTimeLeft;
                            if (timeLeft > 0)
                            {
                                allLua += $@"
                                    wdcrawlerTimer.defTimerText:SetText(""Defend spot timer : {ReadableTime(timeLeft)}"")
                                    wdcrawlerTimer.defTimerText:SetTextColor(0.9, 0.7, 0, 0.9)
                                    wdcrawlerTimer:Show();
                                ";
                            }
                            else
                            {
                                allLua += $@"
                                    wdcrawlerTimer.defTimerText:SetText(""Defend spot timer : --:--"")
                                    wdcrawlerTimer.defTimerText:SetTextColor(0.4, 0.4, 0.4)
                                ";
                            }
                        }
                        // Condtion Timer
                        if (currentStep.StepCompleteConditionModel != null
                            && currentStep.StepCompleteConditionModel.ConditionType == Helpers.CompleteConditionType.Timer)
                        {
                            shouldShowTimer = true;
                            double condTimeLeft = currentStep.StepCompleteConditionModel.GetTimerTimeLeft;
                            if (condTimeLeft > 0)
                            {
                                allLua += $@"
                                wdcrawlerTimer.condTimerText:SetText(""Condition timer : {ReadableTime(condTimeLeft)}"")
                                wdcrawlerTimer.condTimerText:SetTextColor(0.9, 0.7, 0, 0.9)
                                wdcrawlerTimer:Show();
                            ";
                            }
                            else
                            {
                                allLua += $@"
                                wdcrawlerTimer.condTimerText:SetText(""Condition timer : --:--"")
                                wdcrawlerTimer.condTimerText:SetTextColor(0.4, 0.4, 0.4)
                            ";
                            }
                        }
                    }

                    if (!shouldShowTimer)
                    {
                        allLua += $@"
                            wdcrawlerTimer:Hide();
                        ";

                    }

                    _lastStepName = currentStep == null ? "" : currentStep.Name;
                    _lastStepCompletion = currentStep == null ? false : currentStep.IsCompleted;
                    _lastNumberOfSteps = currentNumberOfSteps;
                }
                else // CurrentDungeonProfile is null
                {
                    allLua += $@"
                            wdcrawlerTimer:Hide();
                        ";

                    if (_lastNumberOfSteps > 0)
                    {
                        for (int i = 0; i < _lastNumberOfSteps; i++)
                        {
                            allLua += $@"
                        if wdcrawler.allstepsText{i} and wdcrawler.allstepsText{i}:IsVisible() then
                            wdcrawler.allstepsText{i}:Hide()
                        end";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(allLua))
                {
                    //Logging.Write(allLua);
                    Lua.LuaDoString($@"
                    if wdcrawler then
                        {allLua}
                    end");
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError(ex.ToString());
            }
        }

        private string ReadableTime(double timeMs)
        {
            int totalSeconds = (int)(timeMs / 1000);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds - minutes * 60;
            string leadingZero = seconds < 10 ? "0" : "";
            return $"{minutes}:{leadingZero}{seconds}";
        }

        public void HideFrame()
        {
            Lua.LuaDoString($@"
                if wdcrawler then
                    wdcrawler:Hide();
                end
                if wdcrawlerTimer then
                    wdcrawlerTimer:Hide();
                end
            ");
        }
    }
}
