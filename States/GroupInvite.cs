using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class GroupInvite : State, IState
    {
        public override string DisplayName => "Group Invite";
        private readonly ICache _cache;
        private List<string> groupmembers = new List<string> { "DPSone", "DPStwo", "DPSthree", "Heal" };
        private Timer timer = new Timer();

        public GroupInvite(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!timer.IsReady
                    || !Conditions.InGameAndConnected 
                    || !ObjectManager.Me.IsValid 
                    || Fight.InFight
                    || _cache.IsInInstance)
                {
                    return false;
                }

                timer = new Timer(1000);

                return _cache.ListPartyMember.Count() < 5;
            }
        }

        public override void Run()
        {
            foreach (var player in groupmembers)
            {
                if (!_cache.ListPartyMember.Contains(player))
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
