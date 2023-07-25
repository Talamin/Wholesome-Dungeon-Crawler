using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class ForceIsCastSwitch : State
    {
        public override string DisplayName => "ForceIsCast switch";

        private readonly IEntityCache _entityCache;

        public ForceIsCastSwitch(
            IEntityCache EntityCache)
        {
            _entityCache = EntityCache;
        }

        /* 
         * The sole purpose of this state is to reenable ForceIsCast after regen
         * It should be a top prio state to make sure we always reenable it in time
         */
        public override bool NeedToRun
        {
            get
            {
                if (!_entityCache.Me.HasFoodBuff
                    && !_entityCache.Me.HasDrinkBuff
                    && ObjectManager.Me.ForceIsCast)
                {
                    ObjectManager.Me.ForceIsCast = false;
                    Logger.Log($"Re-enabling casting");
                }

                return false;
            }
        }

        public override void Run() { }
    }
}
