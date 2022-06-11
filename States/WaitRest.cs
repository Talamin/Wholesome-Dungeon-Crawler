using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class WaitRest : State
    {
        public override string DisplayName { get; set; } = "Wait - Rest";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

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

                double totalManaPercent = _entityCache.Me.ManaPercent;
                double totalHealthPercent = _entityCache.Me.HealthPercent;
                int playerCount = 1;
                int playerWithManaCount = _entityCache.Me.Mana > 0 ? 1 : 0;

                foreach (IWoWPlayer player in _entityCache.ListGroupMember)
                {                    
                    if (!player.IsConnected)
                    {
                        Logger.Log($"We wait because Member {player.Name} is not logged into game");
                        return true;
                    }

                    if (player.Dead || player.Auras.ContainsKey(8326))
                    {
                        Logger.Log($"We wait because Member {player.Name} is being dead/spooky");
                        return true;
                    }

                    if (player.Mana > 0)
                    {
                        playerWithManaCount++;
                        totalManaPercent += player.ManaPercent;
                    }
                    totalHealthPercent += player.HealthPercent;
                    playerCount++;

                    if (player.HasDrinkBuff || player.HasFoodBuff)
                    {
                        Logger.Log($"We wait because Member {player.Name} is being thirsty or hungry");
                        return true;
                    }
                    /*
                    if (player.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) >= 40)
                    {
                        Logger.Log($"We wait because Member {player.Name} is being lazy");
                        return true;
                    }
                    */
                }

                if (totalHealthPercent / playerCount < 50)
                {
                    Logger.Log($"We wait because the team needs health");
                    return true;
                }

                if (totalManaPercent / playerWithManaCount < 50)
                {
                    Logger.Log($"We wait because the team needs mana");
                    return true;
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
    }
}
