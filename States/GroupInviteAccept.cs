using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class GroupInviteAccept : State, IState
    {
        public override string DisplayName => "Group Accept";
        private readonly ICache _cache;
        private Timer _stateTimer = new Timer();
        private string _tankName = WholesomeDungeonCrawlerSettings.CurrentSetting.TankName.ToLower().Trim();
        private int _luaResult = 0;

        public GroupInviteAccept(ICache iCache)
        {
            _cache = iCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || !_stateTimer.IsReady
                    || _cache.IsInInstance)
                {

                    return false;
                }

                _luaResult = Lua.LuaDoString<int>($@"
                    for i = 1, 5, 1 do
                        local popup = _G[""StaticPopup"" .. i];
                        if popup and popup:IsVisible() then
                            local popupText = _G[""StaticPopup"" .. i .. ""Text""]:GetText();
                            if string.find(popupText, ""invites you to a group"") then
                                -- We have found an invite popup, make sure it comes from tank
                                if string.find(string.lower(popupText), ""{_tankName}"") then
                                    return i; -- return the popup to click
                                else
                                    return -i; -- return -number to indicate that the invite is not from tank, then decline
                                end
                            end
                        end
                    end
                    return 0; -- no invite popup
                ");

                return _luaResult != 0;
            }
        }

        public override void Run()
        {
            // We have an invite, but not from tank
            if (_luaResult < 0)
            {
                int staticPopupIndex = -_luaResult;
                Lua.LuaDoString($"StaticPopup{staticPopupIndex}Button2:Click();");
                Logger.LogOnce("Denied invite. Make sure the tank name is correctly set in the product settings.");
            }
            else
            // We have an invite from tank
            {
                Lua.LuaDoString($"StaticPopup{_luaResult}Button1:Click()");
                Logger.LogOnce($"Accepted Invite from {_tankName}");
            }
        }
    }
}
