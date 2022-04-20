using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class GroupInvite : State, IState
    {
        public override string DisplayName
        {
            get { return "Group Invite"; }
        }
        public override int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private int _priority;
        private ICache _cache = new Cache();

        //ToDo: Generate  Memberlist through
        //ToDo: UserInterface Use LUA instead of  Party. Function
        //ToDo: Use Lock for LUA  Calls
        //ToDo:Use another Method instead of Sleep

        private List<string> groupmembers = new List<string> { "DPSone", "DPStwo", "DPSthree", "Heal" };
        public override bool NeedToRun
        {
            get
            {
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
                if(!InParty(player))
                {
                    Lua.LuaDoString(Usefuls.WowVersion > 5875
                        ? $@"InviteUnit('{player}');"
                        : $@"InviteByName('{player}');");
                }
                Thread.Sleep(1000);
            }
        }
        private bool InParty(string name)
        {
            return Lua.LuaDoString<bool>($@"
            for i=1,4 do
                if (string.lower(UnitName('party'..i)) == '{name.ToLower()}') then
                    return true;
                end
            end
            return false;
        ");
        }
    }
}
