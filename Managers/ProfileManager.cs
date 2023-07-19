using Newtonsoft.Json;
using robotManager.Helpful;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Managers
{
    class ProfileManager : IProfileManager
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IPathManager _pathManager;
        private readonly IPartyChatManager _partyChatManager;
        private IProfile _currentProfile = null;
        public static readonly string ProfilesDirectoryName = "Wholesome-Dungeon-Crawler-Profiles";

        public List<DungeonModel> AvailableDungeons { get; private set; }
        public IProfile CurrentDungeonProfile => _currentProfile;
        public bool ProfileIsRunning => _currentProfile != null && _currentProfile.CurrentStep != null;

        public ProfileManager(IEntityCache entityCache, ICache cache, IPathManager pathManager, IPartyChatManager partyChatManager)
        {
            _entityCache = entityCache;
            _cache = cache;
            _pathManager = pathManager;
            _partyChatManager = partyChatManager;
        }
        public void Initialize()
        {
            AvailableDungeons = Toolbox.GetListAvailableDungeons();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += LuaEventHandler;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= LuaEventHandler;
            if (_currentProfile != null)
            {
                foreach (IStep step in _currentProfile.GetAllSteps)
                {
                    step.Dispose();
                }
            }
        }

        public void LoadProfile(bool safeWait)
        {
            int waitTime = safeWait ? 2000 : 0;

            UnloadCurrentProfile();

            List<DungeonModel> myContinentDungeons = new List<DungeonModel>();
            bool imDead = _entityCache.Me.IsDead && _entityCache.Me.Auras.ContainsKey(8326);
            bool isHeroic = Lua.LuaDoString<int>($"return GetDungeonDifficulty();") == 2; // for later, in case of mid-dungeon reload

            Logger.Log($"--------- PROFILE MANAGER ---------");
            // Dead, search for closest dungeon entrance
            if (imDead)
            {
                DungeonModel closestDungeonByEntranceLoc = Lists.AllDungeons
                    .OrderBy(x => _entityCache.Me.PositionCorpse.DistanceTo(x.EntranceLoc))
                    .FirstOrDefault();
                myContinentDungeons.Add(closestDungeonByEntranceLoc);
                Logger.Log($"You're dead without a profile loaded. Looking for closest dungeon entrance");
            }
            else
            {
                // Get all dungeons with my map ID
                myContinentDungeons = Lists.AllDungeons
                    .FindAll(d => d.MapId == Usefuls.ContinentId);
                Logger.Log($"{myContinentDungeons.Count} dungeons match your Map ID");
            }

            if (myContinentDungeons.Count > 0)
            {
                Vector3 myPos = _entityCache.Me.PositionWithoutType;
                float closestMatchDistance = float.MaxValue;
                ProfileModel chosenModel = null;

                foreach (DungeonModel dungeonModel in myContinentDungeons)
                {
                    // Search for all profiles in corresponding path
                    DirectoryInfo profilePath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/{ProfilesDirectoryName}/{dungeonModel.Name}");
                    int profilecount = profilePath.GetFiles().Count();
                    if (profilecount > 0)
                    {
                        List<FileInfo> files = profilePath.GetFiles().ToList();
                        files.RemoveAll(file => !file.Name.EndsWith(".json"));
                        List<ProfileModel> profileModels = new List<ProfileModel>();

                        // Deserialize and filter json files
                        foreach (FileInfo file in files)
                        {
                            Logger.Log($"Checking {file.Name}");
                            ProfileModel deserializedProfile = null;
                            try
                            {
                                deserializedProfile = JsonConvert.DeserializeObject<ProfileModel>(
                                    File.ReadAllText(file.FullName),
                                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                            }
                            catch (JsonSerializationException ex)
                            {
                                Logger.LogError($"There was an error when trying to deserialize the profile {file.FullName}.");
                                Logger.LogError($"{ex}");
                                return;
                            }

                            // wrong map id
                            if (deserializedProfile.MapId != dungeonModel.MapId)
                            {
                                Logger.LogError($"The Map ID {deserializedProfile.MapId} should be {dungeonModel.MapId} in the following file: {file.FullName}");
                                continue;
                            }

                            if (deserializedProfile.Faction == FactionType.Alliance && !_cache.IAmAlliance
                                || deserializedProfile.Faction == FactionType.Horde && _cache.IAmAlliance)
                            {
                                continue;
                            }

                            deserializedProfile.DungeonModel = dungeonModel; // assign dungeon model for future use

                            profileModels.Add(deserializedProfile);
                        }

                        if (profileModels.Count <= 0)
                        {
                            Logger.LogError($"No profile found for your faction in folder {profilePath}, leaving dungeon");
                            Logger.Log($"-----------------------------------------");
                            Toolbox.LeaveDungeonAndGroup();
                            return;
                        }

                        if (imDead)
                        {
                            chosenModel = profileModels[0];
                            break;
                        }

                        // Search for closest node in this current folder
                        foreach (ProfileModel profileModel in profileModels)
                        {
                            List<Vector3> dungeonPath = new List<Vector3>();
                            // Search in steps
                            foreach (StepModel stepModel in profileModel.StepModels)
                            {
                                if (stepModel is RegroupModel regroupModel)
                                {
                                    if (regroupModel.RegroupSpot == null)
                                    {
                                        Logger.LogError($"WARNING : The step {regroupModel.Name} doesn't have a position!");
                                        continue;
                                    }
                                    dungeonPath.Add(regroupModel.RegroupSpot);
                                }
                                if (stepModel is MoveAlongPathModel moveAlongPathModel)
                                {
                                    dungeonPath.AddRange(moveAlongPathModel.Path);
                                }
                            }

                            Vector3 closestNodeInModel = dungeonPath
                                .OrderBy(node => node.DistanceTo(myPos))
                                .FirstOrDefault();
                            if (closestNodeInModel != null && closestNodeInModel.DistanceTo(myPos) < closestMatchDistance)
                            {
                                closestMatchDistance = closestNodeInModel.DistanceTo(myPos);
                                chosenModel = profileModel;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError($"No profile found in potential folder {profilePath}");
                    }
                }

                if (chosenModel == null)
                {
                    Logger.LogError($"No profile found for Map ID {Usefuls.ContinentId}, leaving dungeon");
                    Logger.Log($"-----------------------------------------");
                    Toolbox.LeaveDungeonAndGroup();
                    return;
                }

                // A profile has been found
                string fileName = $"{chosenModel.ProfileName.Replace(" ", "_")}_{chosenModel.Faction}";
                Logger.Log($"Selected {fileName} by closest node from the {chosenModel.DungeonName} folder.");
                _currentProfile = new Profile(chosenModel, _entityCache, _pathManager, _partyChatManager, this, fileName);
                string log = $"Dungeon Profile loaded: {chosenModel.ProfileName}, ";
                log += $"MapID {chosenModel.MapId}, ";
                log += $"{chosenModel.StepModels.Count()} steps, ";
                log += $"{chosenModel.DeathRunPath.Count()} deathrun nodes, ";
                log += $"{chosenModel.OffMeshConnections.Count()} offmesh connections, ";
                log += $"Dungeon model ID: {chosenModel.DungeonModel?.DungeonId}";
                Logger.Log(log);
                Thread.Sleep(waitTime);
                Logger.Log($"-----------------------------------------");
                _currentProfile.SetFirstLaunchStep();
                return;
            }
        }

        public void UnloadCurrentProfile()
        {
            if (_currentProfile != null)
            {
                Logger.Log($"Unloading profile {_currentProfile.FileName}");
                _currentProfile.Dispose();
            }
            _currentProfile = null;
        }

        private void LuaEventHandler(string eventid, List<string> args)
        {
            switch (eventid)
            {
                case "PLAYER_LEVEL_UP":
                    AvailableDungeons = Toolbox.GetListAvailableDungeons();
                    break;
            }
        }
    }
}
