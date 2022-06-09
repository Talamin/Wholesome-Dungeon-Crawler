using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
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
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaWithArgs_OnEventsLuaStringWithArgs;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }
        public void Initialize()
        {
            if (ObjectManager.Me.IsInGroup)
            {
                CachePartyMemberChanged();
            }
            CacheListPartyMemberGuid();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaWithArgs_OnEventsLuaStringWithArgs;
            OnObjectManagerPulse();
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            IAmTank = ObjectManager.Me.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName;
        }

        private void EventsLuaWithArgs_OnEventsLuaStringWithArgs(string id, List<string> args)
        {
            switch (id)
            {
                case "WORLD_MAP_UPDATE":
                    CacheListPartyMemberGuid();
                    break;
                case "PARTY_MEMBERS_CHANGED":
                    CacheListPartyMemberGuid();
                    CachePartyMemberChanged();
                    break;
                case "PARTY_MEMBER_DISABLE":
                    CacheListPartyMemberGuid();
                    break;
                case "PARTY_MEMBER_ENABLE":
                    CacheListPartyMemberGuid();
                    break;
                case "RAID_ROSTER_UPDATE":
                    CacheListPartyMemberGuid();
                    break;
                case "GROUP_ROSTER_CHANGED":
                    CacheListPartyMemberGuid();
                    break;
                case "PARTY_CONVERTED_TO_RAID":
                    CacheListPartyMemberGuid();
                    break;
                case "RAID_TARGET_UPDATE":
                    CacheListPartyMemberGuid();
                    break;
            }
        }

        public event TankOMHandler OnTankEnteringOM;

        public IWoWUnit Target { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = Cache(new WoWUnit(0));
        public IWoWLocalPlayer Me { get; private set; } = Cache(new WoWLocalPlayer(0));
        public IWoWUnit[] EnemyUnitsTargetingGroup { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsLootable { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsList { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer[] ListGroupMember { get; private set; } = new IWoWPlayer[0];
        public List<string> ListPartyMemberNames { get; private set; } = new List<string>();
        public IWoWPlayer TankUnit { get; private set; }
        private List<ulong> ListPartyMemberGuid { get; set; } = new List<ulong>();
        private ulong TankGuid { get; set; }
        public bool IAmTank { get; private set; }


        //Groupplay  Section
        public IWoWUnit[] EnemyAttackingGroup { get; private set; } = new IWoWUnit[0];

        //private float EnemiesNearTargetRange;
        //private float EnemiesNearMeRange;
        //private float InterruptibleEnemiesRange;

        private static IWoWLocalPlayer Cache(WoWLocalPlayer player) => new CachedWoWLocalPlayer(player);
        private static IWoWUnit Cache(WoWUnit unit) => new CachedWoWUnit(unit);
        private static IWoWPlayer Cache(WoWPlayer player) => new CachedWoWPlayer(player);
        private static bool Reachable(Vector3 a, Vector3 b) => !TraceLine.TraceLineGo(a, b, CGWorldFrameHitFlags.HitTestSpellLoS);
        private static bool Reachable(Vector3 a, Vector3 b, ref bool? cachedReachable)
        {
            if (cachedReachable is bool reachable)
            {
                return reachable;
            }
            reachable = Reachable(a, b);
            cachedReachable = reachable;
            return reachable;
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            WoWLocalPlayer player;
            IWoWLocalPlayer cachedPlayer;
            IWoWUnit cachedTarget, cachedPet;
            List<WoWUnit> units;
            List<WoWPlayer> playerUnits;
            //IWoWLocalPlayer cachedPlayer;

            lock (cacheLock)
            {
                player = ObjectManager.Me;
                cachedPlayer = Cache(player);

                cachedTarget = Cache(new WoWUnit(0));
                var targetObjectBaseAddress = ObjectManager.GetObjectByGuid(player.Target).GetBaseAddress;
                if (targetObjectBaseAddress != 0)
                {
                    var target = new WoWUnit(targetObjectBaseAddress);
                    if (Reachable(cachedPlayer.PositionWithoutType, target.PositionWithoutType))
                    {
                        cachedTarget = Cache(target);
                    }
                }

                cachedPet = Cache(ObjectManager.Pet);
                units = ObjectManager.GetObjectWoWUnit();
                playerUnits = ObjectManager.GetObjectWoWPlayer();
            }
            var enemyUnitsLootable = new List<IWoWUnit>(units.Count);
            var enemyAttackingGroup = new List<IWoWUnit>(units.Count);
            var enemyUnits = new List<IWoWUnit>(units.Count);
            var listGroupMember = new List<IWoWPlayer>(units.Count);

            var targetGuid = cachedTarget.Guid;
            var playerPosition = cachedPlayer.PositionWithoutType;

            IWoWPlayer tankUnit = null;

            foreach (var play in playerUnits)
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
                ListGroupMember = listGroupMember.ToArray();
            }

            if (TankUnit == null && tankUnit != null)
            {
                OnTankEnteringOM();
            }

            TankUnit = tankUnit;

            foreach (var unit in units)
            {
                var unitGuid = unit.Guid;
                IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : Cache(unit);
                bool? cachedReachable = unitGuid == targetGuid ? true : (bool?)null;
                var unitPosition = unit.PositionWithoutType;

                if (!unit.IsDead && unit.Level > 1 && unit.Reaction <= Reaction.Neutral && unit.PositionWithoutType.DistanceTo(playerPosition) <= 100)
                {
                    enemyUnits.Add(cachedUnit);
                }

                if (!unit.IsAlive || unit.NotSelectable)
                {
                    continue;
                }

                if (unit.IsTargetingPartyMember && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyAttackingGroup.Add(cachedUnit);
                }
            }


            Me = cachedPlayer;
            Target = cachedTarget;
            Pet = cachedPet;

            EnemyUnitsLootable = enemyUnitsLootable.ToArray();
            EnemyAttackingGroup = enemyAttackingGroup.ToArray();
            EnemyUnitsList = enemyUnits.ToArray();
            /*
            if (watch.ElapsedMilliseconds > 50)
                Logger.LogError($"Entity cache pulse took {watch.ElapsedMilliseconds}");
            */
        }

        private void CacheListPartyMemberGuid()
        {
            Task.Delay(1000).ContinueWith(x =>
            {
                List<ulong> partyMembers = new List<ulong>();
                TankGuid = 0;
                foreach (WoWPlayer p in Party.GetParty())
                {
                    partyMembers.Add(p.Guid);
                    if (p.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                    {
                        TankGuid = p.Guid;
                    }
                }
                partyMembers.Add(Me.Guid);
                ListPartyMemberGuid = partyMembers;
            });
        }

        private void ClearCachedLists()
        {
            ListPartyMemberNames.Clear();
        }

        private void CachePartyMemberChanged()
        {
            //Debugger.Break();
            ClearCachedLists();
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
            }
        }
    }
}
