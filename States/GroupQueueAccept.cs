
using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class GroupQueueAccept : State, IState
    {
        public override string DisplayName => "GroupQueue Accept";
        private readonly ICache _cache;

        public GroupQueueAccept(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
        }

        private Timer timer = new Timer();

        //private readonly bool LFDRoleCheckPopupAcceptButton = Lua.LuaDoString<bool>("return LFDRoleCheckPopupAcceptButton:IsVisible()");
        public override bool NeedToRun
        {
            get
            {
                if (!timer.IsReady
                    || !Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || _cache.IsInInstance
                    || _cache.ListPartyMember.Count() < 4) //changed from 4
                {
                    return false;
                }
                timer = new Timer(1000);

                return _cache.LFGRoleCheckShown;

            }
 
        }   

        public override void Run()
        {
            Logger.Log("Accept Role Queue Check!");
            Lua.LuaDoString("LFDRoleCheckPopupAcceptButton:Click()");
        }

    }
}
