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
                /*
                foreach (string playername in _entityCache.ListPartyMemberNames)
                {
                    if (!_entityCache.ListGroupMember.Any(y => y.Name == playername))
                    {
                        Logger.Log($"We wait because Member {playername} is not in  ObjectManager");
                        return true;
                    }
                }
                */
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
                    if (player.HasDrinkBuff || player.HasFoodBuff)
                    {
                        Logger.Log($"We wait because Member {player.Name} is being thirsty or  Hungry");
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
