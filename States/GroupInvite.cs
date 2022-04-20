using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wholesome_Dungeon_Crawler.Helpers;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

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
        
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected || !ObjectManager.Me.IsValid || Fight.InFight)
                {
                    return false;
                }
                if (!_cache.IsInInstance)
                {
                    return false;
                }

                if(Party.GetPartyNumberPlayers() < 5)
                {
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {            
            foreach(var player in groupmembers)
            {
                _cache.PartymemberName = player;
                if(!_cache.InParty)
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
