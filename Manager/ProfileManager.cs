using Newtonsoft.Json;
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
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Manager
{
    class ProfileManager : IProfileManager
    {
        private object profileLock = new object();

        public Profile dungeonProfile { get; private set; }

        private readonly IEntityCache _entityCache;

        public ProfileManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }
        public void Initialize()
        {
            CachePlayerEnteringWorld();
            //starting with Event Substcription
            EventsLua.AttachEventLua("PLAYER_ENTERING_WORLD", m => CachePlayerEnteringWorld());
        }

        private void CachePlayerEnteringWorld()
        {
            lock (profileLock)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            CheckactualDungeonProfileInList();
            CheckandChooseactualDungeon();

            if (CheckactualDungeonProfileInList() && actualDungeon != null)
            {
                var profilePath = System.IO.Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/WholesomeDungeonCrawler/{actualDungeon.Name}");
                var profilecount = profilePath.GetFiles().Count();
                if (profilecount > 0)
                {
                    var files = profilePath.GetFiles();
                    var chosenFile = files[new Random().Next(0, files.Length)];
                    var profile = chosenFile.FullName;
                    dungeonProfile = new Profile();
                    dungeonProfile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profile), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                    Logger.Log($"Dungeon Profile loaded: {dungeonProfile.Name}.{Environment.NewLine} with the DungeonID { dungeonProfile.Dungeon.DungeonId}.{ Environment.NewLine} with at Total Steps { dungeonProfile.Steps.Count()}.{ Environment.NewLine}");
                    //PathFinder.OffMeshConnections.AddRange(dungeonProfile.offMeshConnections); <-- in its current state, Profile doesn´t hold any Offmeshes
                }
                Logger.Log("No Profile found!");
                return;
            }
        }

        private void CheckandChooseactualDungeon()
        {
            if(CheckactualDungeonProfileInList())
            {
                if(Lists.AllDungeons.Count(d => d.MapId == Usefuls.ContinentId) > 1)
                {
                    actualDungeon = Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).OrderBy(o => o.Start.DistanceTo(_entityCache.Me.PositionWithoutType)).FirstOrDefault();
                }
                if (Lists.AllDungeons.Count(d => d.MapId == Usefuls.ContinentId) == 1)
                {
                    actualDungeon = Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).FirstOrDefault();
                }
            }
        }
        private bool CheckactualDungeonProfileInList()
        {
            return Lists.AllDungeons.Any(d => d.MapId == Usefuls.ContinentId);
        }

        public void Dispose()
        {

        }
    }
}
