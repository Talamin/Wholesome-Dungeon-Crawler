using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class WaitRest : State
    {
        public override string DisplayName => Name;
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public WaitRest(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        private string Name = "Wait - Rest";

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

                foreach(var playername in _cache.ListPartyMember)
                {
                    if(!_entityCache.ListGroupMember.Any(y=> y.Name == playername))
                    {
                        Name = ($"We wait because Member {playername} is not in  ObjectManager");
                        return true;
                    }
                }

                foreach(var player in _entityCache.ListGroupMember)
                {                  
                    if(!player.IsConnected)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is not logged into game");
                        return true;
                    }
                    if(player.Dead && player.Guid != _entityCache.TankUnit.Guid)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is being dead");
                        return true;
                    }
                    if(player.HasDrinkBuff && player.Guid != _entityCache.TankUnit.Guid)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is being thirsty");
                        return true;
                    }
                    if(player.HasFoodBuff && player.Guid != _entityCache.TankUnit.Guid)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is being hungry");
                        return true;
                    }
                    if(player.Auras.ContainsKey(8326) && player.Guid != _entityCache.TankUnit.Guid)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is being spooky");
                        return true;
                    }
                    if(player.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) >= 40 && player.Guid != _entityCache.TankUnit.Guid)
                    {
                        DisplayName = ($"We wait because Member {player.Name} is being lazy");
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
