using robotManager.FiniteStateMachine;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
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
        private Timer timer = new Timer(1000);

        public GroupInvite(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
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
                    || !_entityCache.IAmTank)
                {
                    return false;
                }

                timer = new Timer(5000);

                return _entityCache.ListPartyMemberNames.Count() < WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers.Count;
            }
        }

        public override void Run()
        {
            string[] playersToInvite = WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers
                .Where(playerName => !_entityCache.ListPartyMemberNames.Contains(playerName))
                .ToArray();
            Logger.LogOnce($"Inviting: {string.Join(", ", playersToInvite)}");

            foreach (string player in playersToInvite)
            {
                Lua.LuaDoString(Usefuls.WowVersion > 5875
                    ? $@"InviteUnit('{player}');"
                    : $@"InviteByName('{player}');");
                Thread.Sleep(500);
            }
        }
    }
}
