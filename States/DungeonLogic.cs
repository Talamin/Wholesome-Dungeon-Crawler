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
                if (_profileManager.CurrentDungeonProfile?.CurrentStep == null)
                {
                    DisplayName = $"DungeonLogic: None";
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

                if(_entityCache.Me.Dead)
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
                DisplayName = $"DungeonLogic: {_profileManager.CurrentDungeonProfile.CurrentStep.Name}";
                _profileManager.CurrentDungeonProfile.CurrentStep.Run();
            }
        }
    }
}
