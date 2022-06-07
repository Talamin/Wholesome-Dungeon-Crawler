using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    internal class RejoinTank : State
    {
        public override string DisplayName => "Rejoin Tank";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private readonly IPartyChatManager _partyChatManager;

        public RejoinTank(
            ICache iCache, 
            IEntityCache EntityCache, 
            IProfileManager profileManager, 
            IPartyChatManager partyChatManager, 
            int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profileManager;
            _partyChatManager = partyChatManager;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || _entityCache.IAmTank
                    || _entityCache.TankUnit != null
                    || _partyChatManager.TankPosition == null
                    || Fight.InFight
                    || !Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
                {
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {

        }
    }
}
