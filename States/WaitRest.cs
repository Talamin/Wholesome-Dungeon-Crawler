using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
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
                    || _cache.IsInInstance)
                {
                    return false;
                }

                return _entityCache.ListGroupMember.Any(y => y.Auras.ContainsKey(433) || y.Auras.ContainsKey(430) || y.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) >= 40);
            }
        }
        public override void Run()
        {
            Thread.Sleep(500);
        }
    }
}
