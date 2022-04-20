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
    class GroupAccept : State, IState
    {
        public override string DisplayName
        {
            get { return "Group Accept"; }
        }

        private readonly ICache _cache;

        public GroupAccept(ICache iCache, int priority)
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
                if (_cache.IsInInstance)
                {
                    return false;
                }
                if(_cache.IsPartyInviteRequest)
                {
                    return true;
                }
                return false;
            }
        }


        public override void Run()
        {
            string StaticPopupText = Lua.LuaDoString<string>("StaticPopup1Text:GetText()");
            if(StaticPopupText.Contains("Tankname"))
            {
                Logger.Log($"Accepting Invite from Tank");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
            }
            else
            {
                Lua.LuaDoString("StaticPopup1Button2:Click()");
            }
        }

    }
}
