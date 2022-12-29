using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class WaitRest : State
    {
        public override string DisplayName { get; set; } = "Wait - Rest";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private Timer _logTimer = new Timer();

        public WaitRest(ICache iCache, IEntityCache EntityCache)
        {
            _cache = iCache;
            _entityCache = EntityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || _entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance)
                {
                    return false;
                }

                foreach (IWoWPlayer player in _entityCache.ListGroupMember)
                {
                    if (!player.IsConnected)
                    {
                        Log($"Waiting for {player.Name} to log into game");
                        _logTimer = new Timer(1000 * 10);
                        return true;
                    }

                    if (player.Dead || player.Auras.ContainsKey(8326))
                    {
                        Log($"Waiting because {player.Name} is dead");
                        _logTimer = new Timer(1000 * 10);
                        return true;
                    }

                    if (player.HasDrinkBuff || player.HasFoodBuff)
                    {
                        Log($"Waiting for {player.Name} to regenerate");
                        _logTimer = new Timer(1000 * 10);
                        return true;
                    }
                }

                return false;
            }
        }
        public override void Run()
        {
            if (MovementManager.InMovement)
            {
                MovementManager.StopMove();
            }

            Thread.Sleep(500);
        }

        private void Log(string message)
        {
            if (_logTimer.IsReady)
            {
                Logger.Log(message);
            }
        }
    }
}
