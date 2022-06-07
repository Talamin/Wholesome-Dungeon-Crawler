using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class DungeonLogic : State, IState
    {
        public override string DisplayName => "DungeonLogic";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public DungeonLogic(ICache iCache, IEntityCache iEntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
        }
        public override bool NeedToRun
        {
            get
            {
                if (_profileManager.CurrentDungeonProfile?.CurrentStep == null)
                {
                    Logger.Log($"DungeonLogic: None");
                }

                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null)
                {
                    return false;
                }

                if (!_entityCache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.Me.Dead)
                {
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {
            if (_profileManager.CurrentDungeonProfile.CurrentStep.IsCompleted)
            {
                _profileManager.CurrentDungeonProfile.SetCurrentStep();
            }
            else
            {
                Logger.Log($"DungeonLogic: {_profileManager.CurrentDungeonProfile.CurrentStep.Name}");
                _profileManager.CurrentDungeonProfile.CurrentStep.Run();
            }
        }
    }
}
