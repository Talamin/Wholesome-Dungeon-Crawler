using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class GroupInviteAccept : State, IState
    {
        public override string DisplayName => "Group Accept";
        private readonly ICache _cache;

        public GroupInviteAccept(ICache iCache, int priority)
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
                    || _cache.IsInInstance)
                {

                    return false;
                }

                return _cache.IsPartyInviteRequest;
            }
        }


        public override void Run()
        {
            string StaticPopupText = Lua.LuaDoString<string>("return StaticPopup1Text:GetText()");
            if (StaticPopupText.Contains("Tank"))
            {
                Logger.Log($"Accepting Invite from Tank");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                Logger.Log("Accepted Invite");
                return;
            }
            Lua.LuaDoString("StaticPopup1Button2:Click()");
            Logger.Log("Denied Invite");
        }

    }
}
