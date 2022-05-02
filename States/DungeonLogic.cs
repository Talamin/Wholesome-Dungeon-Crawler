using Newtonsoft.Json;
using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class DungeonLogic : State, IState
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private readonly ILogicRunner _logicRunner;
        public DungeonLogic(ICache iCache, IEntityCache iEntityCache, IProfileManager profilemanager, ILogicRunner logicRunner, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
            _logicRunner = logicRunner;
            Priority = priority;
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

                return false;
            }
        }

        public override void Run()
        {
        }
    }
}
