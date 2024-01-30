using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    internal class EntityCache : IEntityCache
    {
        private object cacheLock = new object();
        private bool IAmShaman;

        public EntityCache()
        {
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }
        public void Initialize()
        {
            IAmShaman = ObjectManager.Me.WowClass == WoWClass.Shaman;
            CachePartyMembersInfo();
            OnObjectManagerPulse();
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            IAmTank = WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Tank;
        }

        public ICachedWoWUnit Target { get; private set; } = Cache(new WoWUnit(0));
        public ICachedWoWUnit Pet { get; private set; } = Cache(new WoWUnit(0)); 
        public ICachedWoWLocalPlayer Me { get; private set; } = Cache(new WoWLocalPlayer(0));
        public ICachedWoWUnit[] EnemyUnitsList { get; private set; } = new ICachedWoWUnit[0];

        public ICachedWoWUnit[] InterestingUnitsList { get; private set; } = new ICachedWoWUnit[0];

        public ICachedWoWPlayer[] ListGroupMember { get; private set; } = new ICachedWoWPlayer[0];
        public List<string> ListPartyMemberNames { get; private set; } = new List<string>();
        public ICachedWoWUnit[] EnemiesAttackingGroup { get; private set; } = new ICachedWoWUnit[0];
        public ICachedWoWPlayer TankUnit { get; private set; }
        private string _tankName { get; set; }
        public bool IAmTank { get; private set; }
        public List<ICachedWoWUnit> NpcsToDefend { get; private set; } = new List<ICachedWoWUnit>();
        public List<ICachedWoWUnit> LootableUnits { get; private set; } = new List<ICachedWoWUnit>();
        public void AddNpcIdToDefend(int npcId) => _npcToDefendEntries.Add(npcId);
        public void ClearNpcListIdToDefend() => _npcToDefendEntries.Clear();

        private static ICachedWoWLocalPlayer Cache(WoWLocalPlayer player) => new CachedWoWLocalPlayer(player);
        private static ICachedWoWUnit Cache(WoWUnit unit) => new CachedWoWUnit(unit);
        private static ICachedWoWPlayer Cache(WoWPlayer player) => new CachedWoWPlayer(player);

        private List<string> _petnames = new List<string>();
        private List<int> _npcToDefendEntries = new List<int>();

        private void OnObjectManagerPulse()
        {
            try
            {
                Stopwatch watchTotal = Stopwatch.StartNew();
                Stopwatch watchInit = Stopwatch.StartNew();
                ICachedWoWLocalPlayer cachedMe;
                ICachedWoWUnit cachedTarget, cachedPet;
                List<WoWUnit> units;
                List<WoWPlayer> playerUnits;

                if (!Conditions.InGameAndConnected)
                {
                    return;
                }

                lock (cacheLock)
                {
                    cachedMe = Cache(ObjectManager.Me);
                    cachedTarget = ObjectManager.Target.Guid != 0 ? Cache(ObjectManager.Target) : Cache(new WoWUnit(0)); // Is occasionally slow with shaman for some reason
                    cachedPet = IAmShaman ? Cache(new WoWUnit(0)) : Cache(ObjectManager.Pet);
                    units = ObjectManager.GetObjectWoWUnit();
                    playerUnits = ObjectManager.GetObjectWoWPlayer();
                }

                long initTime = watchInit.ElapsedMilliseconds;
                Stopwatch playersWatch = Stopwatch.StartNew();

                var enemyAttackingGroup = new List<ICachedWoWUnit>();
                var enemyUnits = new List<ICachedWoWUnit>();
                var interestingUnits = new List<ICachedWoWUnit>();
                var listGroupMember = new List<ICachedWoWPlayer>();
                var petNames = new List<string>();

                var targetGuid = cachedTarget.Guid;
                var playerPosition = cachedMe.PositionWT;

                ICachedWoWPlayer tankUnit = null;

                foreach (WoWPlayer play in playerUnits)
                {
                    ICachedWoWPlayer cachedplayer = Cache(play);
                    if (ListPartyMemberNames.Contains(cachedplayer.Name))
                    {
                        listGroupMember.Add(cachedplayer);
                        if (cachedplayer.Name == _tankName)
                        {
                            tankUnit = cachedplayer;
                        }
                    }
                }

                ListGroupMember = listGroupMember.ToArray();

                long playersTime = playersWatch.ElapsedMilliseconds;
                Stopwatch enemiesWatch = Stopwatch.StartNew();

                NpcsToDefend.Clear();
                LootableUnits.Clear();
                TankUnit = tankUnit;

                List<string> allTeamNames = new List<string> { cachedMe.Name };
                allTeamNames.AddRange(ListGroupMember.Select(lgm => lgm.Name));

                foreach (WoWUnit unit in units)
                {
                    // Ignored mobs from list
                    if (Lists.IgnoredMobs.Contains(unit.Entry))
                    {
                        continue;
                    }

                    

                    ulong unitGuid = unit.Guid;
                    ICachedWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : Cache(unit);
                    bool? cachedReachable = unitGuid == targetGuid ? true : (bool?)null;
                    Vector3 unitPosition = unit.PositionWithoutType;

                    if (Lists.InterestingUnits.Contains(unit.Entry))
                    {
                        interestingUnits.Add(cachedUnit);
                    }

                    if (unit.IsPet && unit.Reaction > Reaction.Neutral)
                    {
                        petNames.Add(unit.Name);
                        continue;
                    }

                    if (unit.IsAlive && _npcToDefendEntries.Contains(unit.Entry))
                    {
                        NpcsToDefend.Add(cachedUnit);
                        continue;
                    }

                    if (!unit.IsAlive && unit.IsLootable)
                    {
                        LootableUnits.Add(cachedUnit);
                    }

                    if (!unit.IsAlive || unit.NotSelectable)
                    {
                        continue;
                    }

                    if (unit.Level > 1
                        && unit.Reaction <= Reaction.Neutral
                        && unit.PositionWithoutType.DistanceTo(playerPosition) <= 70)
                    {
                        enemyUnits.Add(cachedUnit);
                        WoWUnit unitTarget = unit.TargetObject;
                        if (unitTarget != null
                            && (allTeamNames.Contains(unitTarget.Name) || _petnames.Contains(unitTarget.Name)))
                        {
                            enemyAttackingGroup.Add(cachedUnit);
                        }
                    }
                }

                Me = cachedMe;
                Target = cachedTarget;
                Pet = cachedPet;

                EnemiesAttackingGroup = enemyAttackingGroup.ToArray();
                EnemyUnitsList = enemyUnits.ToArray();
                InterestingUnitsList = interestingUnits.ToArray();
                _petnames = petNames;

                long enemiesTime = enemiesWatch.ElapsedMilliseconds;

                if (watchTotal.ElapsedMilliseconds > 100)
                    Logger.LogError($"[init: {initTime}] [players: {playersTime}] [{enemyUnits.Count} enemies: {enemiesTime}]");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        // Records name of other players even if outside object manager
        private void CachePartyMembersInfo()
        {
            lock (cacheLock)
            {
                try
                {
                    string pList = Lua.LuaDoString<string>(@"
                        plist='';
                        for i=1,4 do
                            local unitName = UnitName('party'..i);
                            local unitGuid = UnitGUID('party'..i);
                            if unitName then
                                plist = plist .. unitName ..',';
                            end
                        end
                        return plist;
                    ");

                    if (string.IsNullOrEmpty(pList))
                    {
                        ListPartyMemberNames.Clear();
                        return;
                    }

                    List<string> luaNames = pList.Remove(pList.Length - 1, 1).Split(',').ToList();
                    List<string> partyNames = new List<string>();
                    foreach (string name in luaNames)
                    {
                        string[] splitNames = name.Split('|');
                        if (name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                        {
                            _tankName = name;
                        }
                        partyNames.Add(name);
                    }

                    if (!Enumerable.SequenceEqual(ListPartyMemberNames, partyNames))
                    {
                        Logger.Log($"Party: {string.Join(", ", partyNames)}");
                    }

                    ListPartyMemberNames = partyNames;
                }
                catch (Exception e)
                {
                    Logger.LogError(e.ToString());
                }
            }
        }

        public void CacheGroupMembers(string trigger)
        {
            CachePartyMembersInfo();
        }
    }
}
