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

                return _profileManager.actualDungeonProfile;
            }
        }

        public override void Run()
        {
            var actualDungeon = _profileManager.actualDungeon;
            var profilePath = System.IO.Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/DungeonCrawler/{actualDungeon.Name}");
            var profilecount = profilePath.GetFiles().Count();
            if (profilecount > 0)
            {
                if (profilecount > 1)
                {
                    Logger.Log($"We found in total {profilecount} profiles, choosing random one!");
                }
                var files = profilePath.GetFiles();
                var chosenFile = files[new Random().Next(0, files.Length)];
                var profile = chosenFile.FullName;

                Profile dungeonProfile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profile), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                Logger.Log($"Dungeon Profile loaded: {dungeonProfile.Name}.{Environment.NewLine} with the DungeonID { dungeonProfile.Dungeon.DungeonId}.{ Environment.NewLine} with at Total Steps { dungeonProfile.Steps.Count()}.{ Environment.NewLine}");
                //PathFinder.OffMeshConnections.AddRange(dungeonProfile.offMeshConnections); <-- in its current state, Profile doesn´t hold any Offmeshes
                dungeonProfile.Load();
                _logicRunner.Pulse();
            }
        }
    }
}
