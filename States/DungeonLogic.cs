using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.States
{
    class DungeonLogic : State, IState
    {
        public override string DisplayName => "Running Dungeon Step";

        private readonly IProfileManager _profileManager;
        private readonly ICache _cache;

        public DungeonLogic(IProfileManager profilemanager,
            ICache cache)
        {
            _profileManager = profilemanager;
            _cache = cache;
        }

        public override bool NeedToRun
        {
            get
            {
                return _cache.IsInInstance
                    && _profileManager.ProfileIsRunning;
            }
        }

        public override void Run()
        {
            if (_profileManager.CurrentDungeonProfile.CurrentStep.IsCompleted)
            {
                _profileManager.CurrentDungeonProfile.AutoSetCurrentStep();
            }
            else
            {
                _profileManager.CurrentDungeonProfile.CurrentStep.Run();
            }
        }
    }
}
