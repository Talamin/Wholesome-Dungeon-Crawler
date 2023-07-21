using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class AntiStuck : State
    {
        public override string DisplayName => "Anti stuck";

        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private readonly ICache _cache;
        private Vector3 _lastPosition;
        private robotManager.Helpful.Timer _antiStuckTimer = null;
        private int _antiStuckTimerTime = 5 * 60 * 1000;

        public AntiStuck(
            ICache cache,
            IEntityCache EntityCache,
            IProfileManager profilemanager)
        {
            _entityCache = EntityCache;
            _profileManager = profilemanager;
            _cache = cache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_cache.IsInInstance
                    || !_entityCache.Me.IsValid
                    || Fight.InFight
                    || !_profileManager.ProfileIsRunning)
                {
                    _antiStuckTimer = null;
                    return false;
                }

                // We haven't moved, start/check timer
                if (_entityCache.Me.PositionWT == _lastPosition)
                {
                    if (_antiStuckTimer == null)
                        _antiStuckTimer = new robotManager.Helpful.Timer(_antiStuckTimerTime);

                    return _antiStuckTimer.IsReady;
                }
                else // We moved, delete timer
                {
                    _lastPosition = _entityCache.Me.PositionWT;
                    _antiStuckTimer = null;
                    return false;
                }
            }
        }

        public override void Run()
        {
            Logger.LogError($"We have been stuck for {_antiStuckTimerTime / 1000} seconds. Teleporting to entrance.");
            _antiStuckTimer = null;
            MovementManager.StopMove();
            Thread.Sleep(3000);
            _profileManager.UnloadCurrentProfile();
            Lua.LuaDoString("LFGTeleport(true);");
            Thread.Sleep(5000);
            Lua.LuaDoString("LFGTeleport(false);");
            Thread.Sleep(5000);
            _profileManager.LoadProfile(true);
        }
    }
}
