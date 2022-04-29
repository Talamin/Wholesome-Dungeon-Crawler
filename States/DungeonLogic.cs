using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class DungeonLogic : State, IState
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly ILogicRunner _logicRunner;
        public DungeonLogic(ICache iCache, IEntityCache iEntityCache, ILogicRunner logicrunner, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _logicRunner = logicrunner;
            Priority = priority;
        }
        public override bool NeedToRun
        {
            get
            {
                if (!_logicRunner.IsFinished //check for finishing the Logicstate before
                && Lists.AllDungeons.Count(d => d.MapId == Usefuls.ContinentId) > 0 //check if we are inside Dungeon
                && !_logicRunner.OverrideNeedToRun) //check if there is no needed Override
                {
                    return true;
                }          

                return false;
            }
        }

        public override void Run()
        {
            var actualDungeon = Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).OrderBy(o => o.Start.DistanceTo(_entityCache.Me.PositionWithoutType)).FirstOrDefault();
            //actualDungeon.Profile.Load();   <<< this is where the Dungeonprofile will be loaded
            //_logicRunner.Pulse();
        }
    }
}
