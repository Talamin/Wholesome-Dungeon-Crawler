using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class GroupProposal : State, IState
    {
        public override string DisplayName => "GroupProposal Accept";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public GroupProposal(ICache iCache, IEntityCache iEntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
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
                    || _entityCache.ListPartyMemberNames.Count() < 4) //changed from 5
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