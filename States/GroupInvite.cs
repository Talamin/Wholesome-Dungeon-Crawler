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
        private readonly IEntityCache _entityCache;
        private List<string> groupmembers = new List<string> { "Tzuki", "DPStwo", "DPSthree", "Heal" };
        private string tankname = "Tank";
        private Timer timer = new Timer(1000);

        public GroupInvite(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
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
                    || _cache.IsInInstance
                    || _entityCache.Me.Name != tankname)
                {
                    return false;
                }

                timer = new Timer(5000);

                return _cache.ListPartyMember.Count() < 4; //changed from 5 for testing
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
