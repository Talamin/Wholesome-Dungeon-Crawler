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
    class GroupProposal : State, IState
    {
        public override string DisplayName => "GroupProposal Accept";
        private readonly ICache _cache;

        public GroupProposal(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
        }

        private Timer timer = new Timer();

        public override bool NeedToRun
        {
            get
            {
                if (!timer.IsReady
                    || !Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _cache.ListPartyMember.Count() < 4) //changed from 5
                {
                    return false;
                }
                timer = new Timer(1000);

                return _cache.LFGProposalShown;

            }

        }

        public override void Run()
        {
            Logger.Log("Accept Dungeon Proposal!");
            Lua.LuaDoString("AcceptProposal()");
        }

    }
}