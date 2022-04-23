using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class GroupInvite : State, IState
    {
        public override string DisplayName
        {
            get { return "Group Invite"; }
        }
        private readonly ICache _cache;

        public GroupInvite(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
        }

        public List<string> groupmembers = new List<string> { "DPSone", "DPStwo", "DPSthree", "Heal" };
        private Timer timer = new Timer(250);

        public override bool NeedToRun
        {
            get
            {
                if (!timer.IsReady)
                {
                    return false;
                }

                if (!Conditions.InGameAndConnected || !ObjectManager.Me.IsValid || Fight.InFight)
                {
                    return false;
                }
                if (_cache.IsInInstance)
                {
                    return false;
                }

                if(_cache.ListPartyMember.Count() < 5)
                {
                    timer = new Timer(5000);
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {            
            foreach(var player in groupmembers)
            {
                if(!_cache.ListPartyMember.Contains(player))
                {
                    Logger.Log($"Inviting {player} to Group");
                    Lua.LuaDoString(Usefuls.WowVersion > 5875
                        ? $@"InviteUnit('{player}');"
                        : $@"InviteByName('{player}');");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
