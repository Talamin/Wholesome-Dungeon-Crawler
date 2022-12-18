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

                _stateTimer = new Timer(1000);

                return Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();"); ;
            }
        }

        public override void Run()
        {
            string staticPopupText = Lua.LuaDoString<string>("return StaticPopup1Text:GetText()");
            if (staticPopupText.ToLower().Contains(WholesomeDungeonCrawlerSettings.CurrentSetting.TankName.ToLower()))
            {
                Logger.LogOnce($"Accepting Invite from {WholesomeDungeonCrawlerSettings.CurrentSetting.TankName}");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                return;
            }

            Lua.LuaDoString("StaticPopup1Button2:Click()");
            Logger.LogOnce("Denied invite. Make sure the tank name is correctly set in the product settings.");
        }

    }
}
