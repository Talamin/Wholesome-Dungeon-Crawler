using robotManager.FiniteStateMachine;
using System.Linq;
using System.Threading;
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
        private Timer _timer = new Timer();

        public GroupProposal(ICache iCache, IEntityCache iEntityCache)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_timer.IsReady
                    || !Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || _entityCache.ListPartyMemberNames.Count() < 4)
                {
                    return false;
                }

                _timer = new Timer(1000);

                return _cache.LFGProposalShown;
            }
        }

        public override void Run()
        {
            Logger.Log("Accepting dungeon proposal");
            Lua.LuaDoString("AcceptProposal()");
            Thread.Sleep(3000);
        }
    }
}