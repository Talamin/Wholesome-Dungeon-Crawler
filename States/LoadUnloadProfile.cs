using robotManager.FiniteStateMachine;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class LoadUnloadProfile : State
    {
        public override string DisplayName => "Load/Unload Profile";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private int _currentContinent = -1;
        private bool _recordDead = ObjectManager.Me.IsDead;

        public LoadUnloadProfile(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
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

                if (ObjectManager.Me.Position.DistanceTo(new robotManager.Helpful.Vector3(0, 0, 0)) < 5)
                {
                    Logger.LogError($"In void. Delaying profile load/unload.");
                    return false;
                }

                // Alive to dead, keep loaded profile for death run
                if (!_recordDead && _entityCache.Me.Dead)
                {
                    Logger.Log($"We died, keeping profile loaded for death run");
                    _recordDead = true;
                    return false;
                }

                // Dead to alive, reload profile to restart from step 0
                if (_recordDead && !_entityCache.Me.Dead)
                {
                    Logger.Log($"We returned. Reloading profile");
                    _recordDead = false;
                    return true;
                }

                // Continent change
                if (_currentContinent != Usefuls.ContinentId && !_recordDead)
                {
                    _currentContinent = Usefuls.ContinentId;
                    Logger.Log($"Continent change detected: {_currentContinent}");
                    return true;
                }

                return false;
            }
        }


        public override void Run()
        {
            _profileManager.LoadProfile(true);
        }
    }
}
