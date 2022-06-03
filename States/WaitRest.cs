using robotManager.FiniteStateMachine;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class WaitRest : State
    {
        public override string DisplayName { get; set; } = "Wait - Rest";
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public WaitRest(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight
                    || _entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || !_cache.IAmTank)
                {
                    return false;
                }

                foreach(string playername in _cache.ListPartyMemberNames)
                {
                    if(!_entityCache.ListGroupMember.Any(y=> y.Name == playername))
                    {
                        DisplayName = $"We wait because Member {playername} is not in  ObjectManager";
                        return true;
                    }
                }

                foreach(IWoWPlayer player in _entityCache.ListGroupMember)
                {                  
                    if(!player.IsConnected)
                    {
                        DisplayName = $"We wait because Member {player.Name} is not logged into game";
                        return true;
                    }
                    if(player.Dead || player.Auras.ContainsKey(8326))
                    {
                        DisplayName = $"We wait because Member {player.Name} is being dead/spooky";
                        return true;
                    }
                    if(player.HasDrinkBuff || player.HasFoodBuff)
                    {
                        DisplayName = $"We wait because Member {player.Name} is being thirsty or  Hungry";
                        return true;
                    }
                    if(player.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) >= 40)
                    {
                        DisplayName = $"We wait because Member {player.Name} is being lazy";
                        return true;
                    }
                }
                return false;
            }
        }
        public override void Run()
        {
            if (MovementManager.InMovement)
                MovementManager.StopMove();

            Thread.Sleep(500);
        }
    }
}
