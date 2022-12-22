﻿using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles;
using wManager.Wow.Helpers;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Managers
{
    class ProfileManager : IProfileManager
    {
        public IProfile CurrentDungeonProfile { get; private set; }

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IPathManager _pathManager;
        private readonly IPartyChatManager _partyChatManager;
        public static readonly string ProfilesDirectoryName = "Wholesome-Dungeon-Crawler-Profiles";

        public ProfileManager(IEntityCache entityCache, ICache cache, IPathManager pathManager, IPartyChatManager partyChatManager)
        {
            _entityCache = entityCache;
            _cache = cache;
            _pathManager = pathManager;
            _partyChatManager = partyChatManager;
        }
        public void Initialize()
        {
            //LoadProfile(false);
        }

        public void Dispose()
        {
        }

        public void LoadProfile(bool safeWait)
        {
            int waitTime = safeWait ? 3000 : 0;

            UnloadCurrentProfile();

            DungeonModel dungeon = CheckandChooseactualDungeon();
            if (dungeon != null)
            {
                DirectoryInfo profilePath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/{ProfilesDirectoryName}/{dungeon.Name}");
                int profilecount = profilePath.GetFiles().Count();
                if (profilecount > 0)
                {
                    List<FileInfo> files = profilePath.GetFiles().ToList();
                    files.RemoveAll(file => !file.Name.EndsWith(".json"));
                    List<ProfileModel> profileModels = new List<ProfileModel>();

                    // Deserialize and filter json files
                    foreach (FileInfo file in files)
                    {
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
                        if (deserializedProfile.MapId != dungeon.MapId)
                        {
                            Logger.LogError($"The Map ID {deserializedProfile.MapId} should be {dungeon.MapId} in the following file: {file.FullName}");
                            continue;
                        }

                        if (deserializedProfile.Faction == FactionType.Alliance && !_cache.IAmAlliance
                            || deserializedProfile.Faction == FactionType.Horde && _cache.IAmAlliance)
                        {
                            continue;
                        }

                        profileModels.Add(deserializedProfile);
                    }

                    if (profileModels.Count <= 0)
                    {
                        Logger.LogError($"No profile found for your faction in folder {profilePath}, leaving dungeon");
                        Toolbox.LeaveDungeonAndGroup();
                        return;
                    }

                    ProfileModel chosenModel = profileModels[new Random().Next(0, profileModels.Count)];
                    string fileName = $"{chosenModel.ProfileName.Replace(" ", "_")}_{chosenModel.Faction}";
                    Logger.Log($"Randomly selected {fileName} from the {dungeon.Name} folder.");
                    if (chosenModel.OffMeshConnections.Count > 0)
                    {
                        PathFinder.OffMeshConnections.AddRange(chosenModel.OffMeshConnections);
                    }
                    CurrentDungeonProfile = new Profile(chosenModel, _entityCache, _pathManager, _partyChatManager, this, fileName);
                    Logger.Log($"Dungeon Profile loaded: {chosenModel.ProfileName} {Environment.NewLine} MapID {chosenModel.MapId}" +
                        $"{Environment.NewLine} {chosenModel.StepModels.Count()} steps" +
                        $"{Environment.NewLine} {chosenModel.DeathRunPath.Count()} deathrun nodes" +
                        $"{Environment.NewLine} {chosenModel.OffMeshConnections.Count()} offmesh connections");
                    Thread.Sleep(waitTime);
                    CurrentDungeonProfile.SetFirstLaunchStep();
                    return;
                }
                else
                {
                    Logger.LogError($"No profile found in folder {profilePath}, leaving dungeon");
                    Toolbox.LeaveDungeonAndGroup();
                    return;
                }
            }

            Logger.Log($"You're not in a dungeon. Map ID {Usefuls.ContinentId}.");
        }

        public void UnloadCurrentProfile()
        {
            if (CurrentDungeonProfile != null)
            {
                Logger.Log($"Unloading profile {CurrentDungeonProfile.FileName}");
                CurrentDungeonProfile.Dispose();
            }
            CurrentDungeonProfile = null;
        }

        private DungeonModel CheckandChooseactualDungeon()
        {
            if (_entityCache.Me.Dead && !_cache.IsInInstance)
            {
                return Lists.AllDungeons.Where(x => x.ContinentId == Usefuls.ContinentId).OrderBy(x => _entityCache.Me.PositionCorpse.DistanceTo(x.EntranceLoc)).First();
            }

            if (Lists.AllDungeons.Any(d => d.MapId == Usefuls.ContinentId))
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
    }
}
