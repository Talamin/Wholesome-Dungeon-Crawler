using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaWithArgs_OnEventsLuaStringWithArgs;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
        }
        public void Initialize()
        {
            if(ObjectManager.Me.IsInGroup)
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

        public IWoWUnit Target { get; private set; } = Cache(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = Cache(new WoWUnit(0));
        public IWoWLocalPlayer Me { get; private set; } = Cache(new WoWLocalPlayer(0));
        //public IWoWUnit[] EnemyUnitsNearTarget { get; private set; } = new IWoWUnit[0];
        //public IWoWUnit[] EnemyUnitsNearPlayer { get; private set; } = new IWoWUnit[0];
        //public IWoWUnit[] InterruptibleEnemyUnits { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsTargetingPlayer { get; private set; } = new IWoWUnit[0];
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
            var enemyUnitsNearTarget = new List<IWoWUnit>(units.Count);
            var enemyUnitsNearPlayer = new List<IWoWUnit>(units.Count);
            var interruptibleEnemyUnits = new List<IWoWUnit>(units.Count);
            var enemyUnitsTargetingPlayer = new List<IWoWUnit>(units.Count);
            var enemyUnitsLootable = new List<IWoWUnit>(units.Count);
            var enemyAttackingGroup = new List<IWoWUnit>(units.Count);
            var enemyUnits = new List<IWoWUnit>(units.Count);
            var listGroupMember = new List<IWoWPlayer>(units.Count);

            var targetPosition = cachedTarget.PositionWithoutType;
            var targetGuid = cachedTarget.Guid;
            var playerPosition = cachedPlayer.PositionWithoutType;
            var playerGuid = cachedPlayer.Guid;

            bool tankFound = false;

            foreach (var play in playerUnits)
            {
                IWoWPlayer cachedplayer = Cache(play);
                if (ListPartyMemberGuid.Contains(cachedplayer.Guid))
                {
                    listGroupMember.Add(cachedplayer);
                    if (cachedplayer.Guid == TankGuid)
                    {
                        TankUnit = cachedplayer;
                        tankFound = true;
                    }
                }
                ListGroupMember = listGroupMember.ToArray();
            }

            if (!tankFound)
            {
                TankUnit = null;
            }

            foreach (var unit in units)
            {
                var unitGuid = unit.Guid;
                IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : Cache(unit);
                bool? cachedReachable = unitGuid == targetGuid ? true : (bool?)null;
                var unitPosition = unit.PositionWithoutType;

                if (unit.IsLootable && unit.IsDead && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyUnitsLootable.Add(cachedUnit);
                }
                if (!unit.IsDead && unit.Level > 1 && unit.Reaction <= Reaction.Neutral && unit.PositionWithoutType.DistanceTo(playerPosition) <= 100)
                {
                    enemyUnits.Add(cachedUnit);
                }
                if (!unit.IsAlive)
                {
                    continue;
                }

                if (!unit.IsAttackable || unit.NotSelectable)
                {
                    continue;
                }

                if (unitGuid != targetGuid && unit.Target != playerGuid)
                {
                    continue;
                }

                if (unitGuid != targetGuid)
                {
                    enemyUnitsTargetingPlayer.Add(cachedUnit);
                }

                //if (targetPosition.DistanceTo(unitPosition) <= EnemiesNearTargetRange && Reachable(playerPosition, unitPosition, ref cachedReachable))
                //{
                //    enemyUnitsNearTarget.Add(cachedUnit);
                //}

                if (unit.IsTargetingPartyMember && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyAttackingGroup.Add(cachedUnit);
                }

                //var playerDistance = playerPosition.DistanceTo(unitPosition);
                //if (playerDistance <= EnemiesNearMeRange && Reachable(playerPosition, unitPosition, ref cachedReachable))
                //{
                //    enemyUnitsNearPlayer.Add(cachedUnit);
                //}

                //if (playerDistance <= InterruptibleEnemiesRange && Reachable(playerPosition, unitPosition, ref cachedReachable) && unit.CanInterruptCasting)
                //{
                //    interruptibleEnemyUnits.Add(cachedUnit);
                //}
            }

            Me = cachedPlayer;
            Target = cachedTarget;
            Pet = cachedPet;

            //EnemyUnitsNearTarget = enemyUnitsNearTarget.ToArray();
            //EnemyUnitsNearPlayer = enemyUnitsNearPlayer.ToArray();
            //InterruptibleEnemyUnits = interruptibleEnemyUnits.ToArray();
            EnemyUnitsTargetingPlayer = enemyUnitsTargetingPlayer.ToArray();
            EnemyUnitsLootable = enemyUnitsLootable.ToArray();
            EnemyAttackingGroup = enemyAttackingGroup.ToArray();
            EnemyUnitsList = enemyUnits.ToArray();

            if (watch.ElapsedMilliseconds > 50)
                Logger.LogError($"Entity cache pulse took {watch.ElapsedMilliseconds}");
        }

        private void CacheListPartyMemberGuid()
        {
            Thread.Sleep(500);
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
