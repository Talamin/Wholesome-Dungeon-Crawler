using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class GroupAccept : State, IState
    {
        public override string DisplayName
        {
            get { return "Group Accept"; }
        }
        public override int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private int _priority;
        private ICache _cache = new Cache();
        public override bool NeedToRun
        {
            get
            {
                if(_cache.IsInInstance)
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
                Lua.LuaDoString("StaticPopup1Button1:Click()");
            }
            else
            {
                Lua.LuaDoString("StaticPopup1Button2:Click()");
            }
        }

    }
}
