using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    class ProfileManager : IProfileManager
    {
        private object profileLock = new object();

        public IProfile CurrentDungeonProfile { get; private set; }

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public ProfileManager(IEntityCache entityCache, ICache cache)
        {
            _entityCache = entityCache;
            _cache = cache;
        }
        public void Initialize()
        {
            LoadProfile(false);
            //starting with Event Substcription
            //EventsLua.AttachEventLua("PLAYER_ENTERING_WORLD", m => CachePlayerEnteringWorld());
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaWithArgs_OnEventsLuaStringWithArgs;
        }

        private void EventsLuaWithArgs_OnEventsLuaStringWithArgs(string id, List<string> args)
        {
            switch (id)
            {
                case "PLAYER_ENTERING_WORLD":
                    LoadProfile(true);
                    break;
            }
        }

        private void LoadProfile(bool safeWait)
        {
            int waitTime = safeWait ? 3000 : 0;

            Task.Delay(waitTime).ContinueWith(t =>
            {
                DungeonModel dungeon = CheckandChooseactualDungeon();
                if (dungeon != null)
                {
                    var profilePath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/WholesomeDungeonCrawler/{dungeon.Name}");
                    var profilecount = profilePath.GetFiles().Count();
                    if (profilecount > 0)
                    {
                        var files = profilePath.GetFiles();
                        var chosenFile = files[new Random().Next(0, files.Length)];
                        Logger.Log($"Randomly selected {chosenFile.Name} from the {dungeon.Name} folder.");
                        var profile = chosenFile.FullName;
                        var deserializedProfile = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(profile), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        if (deserializedProfile.MapId == dungeon.MapId)
                        {
                            CurrentDungeonProfile = new Profile(deserializedProfile, _entityCache);
                            Logger.Log($"Dungeon Profile loaded: {deserializedProfile.Name}.{Environment.NewLine} with the MapID { deserializedProfile.MapId}.{ Environment.NewLine} with at Total Steps { deserializedProfile.StepModels.Count()}.{ Environment.NewLine} with a { deserializedProfile.DeathRunPath.Count()}.{ Environment.NewLine} Steps Deathrun and { deserializedProfile.OffMeshConnections.Count()}.{ Environment.NewLine} OffmeshConnections");
                            CurrentDungeonProfile.SetFirstLaunchStep();
                            return;
                            //PathFinder.OffMeshConnections.AddRange(dungeonProfile.offMeshConnections); <-- in its current state, Profile doesn´t hold any Offmeshes
                        }
                        else
                        {
                            Logger.Log($"Dungeon Profile not loaded: {deserializedProfile.Name}.{Environment.NewLine} with the DungeonID { deserializedProfile.MapId} did not match the dungeon id of your current dungeon {dungeon.Name}: {dungeon.MapId}.");
                            return;
                        }
                    }
                }
                Logger.Log("No Profile found!");
                CurrentDungeonProfile = null;
            });
        }

        private DungeonModel CheckandChooseactualDungeon()
        {
            if (_entityCache.Me.Dead && !_cache.IsInInstance)
            {
                return Lists.AllDungeons.Where(x => x.ContinentId == Usefuls.ContinentId).OrderBy(x => _entityCache.Me.PositionCorpse.DistanceTo(x.EntranceLoc)).First();
            }
            if (CheckactualDungeonProfileInList())
            {
                if (Lists.AllDungeons.Count(d => d.MapId == Usefuls.ContinentId) > 1)
                {
                    return Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).OrderBy(o => o.Start.DistanceTo(_entityCache.Me.PositionWithoutType)).FirstOrDefault();
                }
                if (Lists.AllDungeons.Count(d => d.MapId == Usefuls.ContinentId) == 1)
                {
                    return Lists.AllDungeons.Where(d => d.MapId == Usefuls.ContinentId).FirstOrDefault();
                }
            }
            return null;
        }
        private bool CheckactualDungeonProfileInList()
        {
            return Lists.AllDungeons.Any(d => d.MapId == Usefuls.ContinentId);
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaWithArgs_OnEventsLuaStringWithArgs;
        }
    }
}
