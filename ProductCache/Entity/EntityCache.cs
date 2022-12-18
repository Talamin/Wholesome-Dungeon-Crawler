using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= OnEventsLuaStringWithArgs;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }
        public void Initialize()
        {
            CachePartyMemberChanged();
            CacheListPartyMemberGuid();
            OnObjectManagerPulse();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += OnEventsLuaStringWithArgs;
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            IAmTank = ObjectManager.Me.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName;
        }

        private void OnEventsLuaStringWithArgs(string id, List<string> args)
        {
            switch (id)
            {
                case "WORLD_MAP_UPDATE":
                    CacheGroupMembers();
                    Toolbox.StopAllMoves();
                    break;
                case "PLAYER_ENTERING_WORLD":
                    CacheGroupMembers();
                    Toolbox.StopAllMoves();
                    break;
                case "PARTY_MEMBERS_CHANGED":
                    CacheGroupMembers();
                    break;
                case "PARTY_MEMBER_DISABLE":
                    CacheGroupMembers();
                    break;
                case "PARTY_MEMBER_ENABLE":
                    CacheGroupMembers();
                    break;
                case "RAID_ROSTER_UPDATE":
                    CacheGroupMembers();
                    break;
                case "GROUP_ROSTER_CHANGED":
                    CacheGroupMembers();
                    break;
                case "PARTY_CONVERTED_TO_RAID":
                    CacheGroupMembers();
                    break;
                case "RAID_TARGET_UPDATE":
                    CacheGroupMembers();
                    break;
            }
        }

        public event TankOMHandler OnTankEnteringOM;

        public IWoWUnit Target { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit[] GroupPets { get; private set; } = new IWoWUnit[0];
        public IWoWLocalPlayer Me { get; private set; } = Cache(new WoWLocalPlayer(0));
        public IWoWUnit[] EnemyUnitsList { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer[] ListGroupMember { get; private set; } = new IWoWPlayer[0]; //contains everyone in the cache, except myself
        public List<string> ListPartyMemberNames { get; private set; } = new List<string>();
        public IWoWUnit[] EnemiesAttackingGroup { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer TankUnit { get; private set; }
        private List<ulong> ListPartyMemberGuid { get; set; } = new List<ulong>();
        private ulong TankGuid { get; set; }
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
                if (ListPartyMemberGuid.Contains(cachedplayer.Guid))
                {
                    listGroupMember.Add(cachedplayer);
                    if (cachedplayer.Guid == TankGuid)
                    {
                        tankUnit = cachedplayer;
                    }
                }
            }
            ListGroupMember = listGroupMember.ToArray();

            if (TankUnit == null && tankUnit != null)
            {
                OnTankEnteringOM?.Invoke();
            }

            NpcsToDefend.Clear();
            LootableUnits.Clear();
            TankUnit = tankUnit;

            List<ulong> allTeamGuids = new List<ulong>();
            allTeamGuids.Add(cachedMe.Guid);
            allTeamGuids.AddRange(GroupPets.Select(gp => gp.Guid));
            allTeamGuids.AddRange(ListGroupMember.Select(lgm => lgm.Guid));

            foreach (var unit in units)
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

                var unitGuid = unit.Guid;
                IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : Cache(unit);
                bool? cachedReachable = unitGuid == targetGuid ? true : (bool?)null;
                var unitPosition = unit.PositionWithoutType;

                if (ListPartyMemberGuid.Contains(unit.PetOwnerGuid) || unit.IsMyPet)
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
            /*
            if (watch.ElapsedMilliseconds > 50)
                Logger.LogError($"Entity cache pulse took {watch.ElapsedMilliseconds}");*/

        }

        // Records guid of players other than me in object manager
        private void CacheListPartyMemberGuid()
        {
            List<ulong> partyMembersGuids = new List<ulong>();
            TankGuid = 0;
            foreach (WoWPlayer p in Party.GetParty())
            {
                partyMembersGuids.Add(p.Guid);
                if (p.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                {
                    TankGuid = p.Guid;
                }
            }
            ListPartyMemberGuid = partyMembersGuids;
        }

        // Records name of other players even if outside object manager
        private void CachePartyMemberChanged()
        {
            lock (cacheLock)
            {
                string plist = Lua.LuaDoString<string>(@"
                    plist='';
                    for i=1,4 do
                        if (UnitName('party'..i)) then
                            plist = plist .. UnitName('party'..i) ..','
                        end
                    end", "plist");

                if (plist != null && plist.Length > 0)
                {
                    ListPartyMemberNames = plist.Remove(plist.Length - 1, 1).Split(',').ToList();
                }
                else
                {
                    ListPartyMemberNames = new List<string>();
                }
            }
        }

        private void CacheGroupMembers()
        {
            CacheListPartyMemberGuid();
            CachePartyMemberChanged();
            Task.Delay(5000).ContinueWith(x =>
            {
                CacheListPartyMemberGuid();
                CachePartyMemberChanged();
            });
        }
    }
}
