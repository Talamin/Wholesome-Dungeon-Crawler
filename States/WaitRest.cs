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
        public override string DisplayName => "Wait - Rest";
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

                foreach(IWoWPlayer player in _entityCache.ListGroupMember)
                {
                    if(player.Dead)
                    {
                        DisplayName = ($"We wait because of: {player.Name} because of being dead");
                        return true;
                    }
                    if(player.HasDrinkBuff)
                    {
                        DisplayName = ($"We wait because of: {player.Name} because of being thirsty");
                        return true;
                    }
                    if(player.HasFoodBuff)
                    {
                        DisplayName = ($"We wait because of: {player.Name} because of being hungry");
                        return true;
                    }
                    if(player.Auras.ContainsKey(8326))
                    {
                        DisplayName = ($"We wait because of: {player.Name} because of being spooky");
                        return true;
                    }
                    if(player.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) >= 40)
                    {
                        DisplayName = ($"We wait because of: {player.Name} because of being lazy");
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
