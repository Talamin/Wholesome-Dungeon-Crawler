using robotManager.Helpful;
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
        public EntityCache()
        {
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }
        public void Initialize()
        {
            CachePartyMembersInfo();
            OnObjectManagerPulse();
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            IAmTank = ObjectManager.Me.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName;
        }

        public IWoWUnit Target { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit[] GroupPets { get; private set; } = new IWoWUnit[0];
        public IWoWLocalPlayer Me { get; private set; } = Cache(new WoWLocalPlayer(0));
        public IWoWUnit[] EnemyUnitsList { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer[] ListGroupMember { get; private set; } = new IWoWPlayer[0];
        public List<string> ListPartyMemberNames { get; private set; } = new List<string>();
        public IWoWUnit[] EnemiesAttackingGroup { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer TankUnit { get; private set; }
        private List<ulong> _listPartyMemberGuid { get; set; } = new List<ulong>();
        private ulong _tankGuid { get; set; }
        public bool IAmTank { get; private set; }

        private List<int> _npcToDefendEntries = new List<int>();
        public List<IWoWUnit> NpcsToDefend { get; private set; } = new List<IWoWUnit>();
        public List<IWoWUnit> LootableUnits { get; private set; } = new List<IWoWUnit>();
        public void AddNpcIdToDefend(int npcId) => _npcToDefendEntries.Add(npcId);
        public void ClearNpcListIdToDefend() => _npcToDefendEntries.Clear();

        private static IWoWLocalPlayer Cache(WoWLocalPlayer player) => new CachedWoWLocalPlayer(player);
        private static IWoWUnit Cache(WoWUnit unit) => new CachedWoWUnit(unit);
        private static IWoWPlayer Cache(WoWPlayer player) => new CachedWoWPlayer(player);

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            WoWLocalPlayer me;
            IWoWLocalPlayer cachedMe;
            IWoWUnit cachedTarget, cachedPet;
            List<WoWUnit> units;
            List<WoWPlayer> playerUnits;

            if (!Conditions.InGameAndConnected)
            {
                return;
            }

            lock (cacheLock)
            {
                me = ObjectManager.Me;
                cachedMe = Cache(me);

                cachedTarget = Cache(new WoWUnit(0));
                var targetObjectBaseAddress = ObjectManager.GetObjectByGuid(me.Target).GetBaseAddress;
                if (targetObjectBaseAddress != 0)
                {
                    var target = new WoWUnit(targetObjectBaseAddress);
                    cachedTarget = Cache(target);
                }

                cachedPet = Cache(ObjectManager.Pet);
                units = ObjectManager.GetObjectWoWUnit();
                playerUnits = ObjectManager.GetObjectWoWPlayer();
            }

            var enemyAttackingGroup = new List<IWoWUnit>();
            var enemyUnits = new List<IWoWUnit>();
            var listGroupMember = new List<IWoWPlayer>();
            var groupPets = new List<IWoWUnit>();

            var targetGuid = cachedTarget.Guid;
            var playerPosition = cachedMe.PositionWithoutType;

            IWoWPlayer tankUnit = null;

            foreach (WoWPlayer play in playerUnits)
            {
                IWoWPlayer cachedplayer = Cache(play);
                if (_listPartyMemberGuid.Contains(cachedplayer.Guid))
                {
                    listGroupMember.Add(cachedplayer);
                    if (cachedplayer.Guid == _tankGuid)
                    {
                        tankUnit = cachedplayer;
                    }
                }
            }

            ListGroupMember = listGroupMember.ToArray();

            NpcsToDefend.Clear();
            LootableUnits.Clear();
            TankUnit = tankUnit;

            List<ulong> allTeamGuids = new List<ulong>();
            allTeamGuids.Add(cachedMe.Guid);
            allTeamGuids.AddRange(GroupPets.Select(gp => gp.Guid));
            allTeamGuids.AddRange(ListGroupMember.Select(lgm => lgm.Guid));

            foreach (WoWUnit unit in units)
            {
                // Ignore hostile statues in Uldaman until they become animated.
                if (Lists.ForceTargetListInt.Contains(unit.Entry)
                    && ((unit.UnitFlags & UnitFlags.NotAttackable) != 0))
                {
                    continue;
                }
                // Ignore seed pods from Nexus
                if (Lists.IgnoreTargetListInt.Contains(unit.Entry))
                {
                    continue;
                }

                ulong unitGuid = unit.Guid;
                IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : Cache(unit);
                bool? cachedReachable = unitGuid == targetGuid ? true : (bool?)null;
                Vector3 unitPosition = unit.PositionWithoutType;

                if (_listPartyMemberGuid.Contains(unit.PetOwnerGuid) || unit.IsMyPet)
                {
                    groupPets.Add(cachedUnit);
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
                    && unit.PositionWithoutType.DistanceTo(playerPosition) <= 60)
                {
                    enemyUnits.Add(cachedUnit);

                    if (unit.Target != 0
                        && allTeamGuids.Contains(unit.Target))
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
            GroupPets = groupPets.ToArray();

            if (watch.ElapsedMilliseconds > 100)
            {
                Logger.LogError($"Entity cache pulse took {watch.ElapsedMilliseconds}");
            }

        }

        // Records name and GUIDs of other players even if outside object manager
        private void CachePartyMembersInfo()
        {
            lock (cacheLock)
            {
                string pList = Lua.LuaDoString<string>(@"
                    plist='';
                    for i=1,4 do
                        local unitName = UnitName('party'..i);
                        local unitGuid = UnitGUID('party'..i);
                        if unitName then
                            plist = plist .. unitName .. '|' .. tonumber(unitGuid) ..',';
                        end
                    end
                    return plist;
                ");

                if (string.IsNullOrEmpty(pList))
                {
                    ListPartyMemberNames.Clear();
                    _listPartyMemberGuid.Clear();
                    return;
                }

                List<string> namesAndGuid = pList.Remove(pList.Length - 1, 1).Split(',').ToList();
                List<string> partyNames = new List<string>();
                List<ulong> partyGuids = new List<ulong>();
                foreach (string nameAndGuid in namesAndGuid)
                {
                    string[] splitNameAndGuid = nameAndGuid.Split('|');
                    if (splitNameAndGuid.Length != 2)
                    {
                        Logger.LogError($"ERROR: splitNameAndGuid's {nameAndGuid} length wasn't 2!");
                        continue;
                    }

                    if (ulong.TryParse(splitNameAndGuid[1], out ulong guid))
                    {
                        string memberName = splitNameAndGuid[0];
                        if (memberName == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                        {
                            _tankGuid = guid;
                        }
                        partyNames.Add(memberName);
                        partyGuids.Add(guid);
                    }
                    else
                    {
                        Logger.LogError($"ERROR: unit guid {splitNameAndGuid[1]} couldn't be parsed!");
                        continue;
                    }
                }

                if (!Enumerable.SequenceEqual(ListPartyMemberNames, partyNames)
                    || !Enumerable.SequenceEqual(_listPartyMemberGuid, partyGuids))
                {
                    Logger.Log($"Party: {string.Join(", ", partyNames)} with GUIDs {string.Join(", ", partyGuids)}");
                }

                ListPartyMemberNames = partyNames;
                _listPartyMemberGuid = partyGuids;
            }
        }

        public void CacheGroupMembers(string trigger)
        {
            CachePartyMembersInfo();
        }
    }
}
