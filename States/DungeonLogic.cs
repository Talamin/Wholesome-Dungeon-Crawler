using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Manager;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class DungeonLogic : State, IState
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public DungeonLogic(ICache iCache, IEntityCache iEntityCache, IProfileManager profilemanager, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
            Priority = priority;
        }
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight)
                {
                    return false;
                }

                return false;
            }
        }

        public override void Run()
        {
            //switch (_profileManager.CurrentDungeonProfile.CurrentStep.StepType)
            //{
            //    case MoveAlongPath:
            //        RunMove();
            //        break;
            //}
        }
    }
}
