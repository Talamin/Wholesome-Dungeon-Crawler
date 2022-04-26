using robotManager.Helpful;
using System.Collections.Generic;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class EntityCache : IEntityCache
    {
        private object cacheLock = new object();
        public EntityCache()
        {
        }

        public void Dispose() => ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;

        public void Initialize() => ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;

        public IWoWUnit Target { get; private set; }
        public IWoWUnit Pet { get; private set; }
        public IWoWUnit[] EnemyUnitsNearTarget { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsNearPlayer { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] InterruptibleEnemyUnits { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsTargetingPlayer { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsTargetingGroup { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsLootable { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] ListGroupMember { get; private set; } = new IWoWUnit[0];
        public IWoWUnit Me { get; private set; }

        //Groupplay  Section
        public IWoWUnit[] EnemyAttackingGroup { get; private set; } = new IWoWUnit[0];

        private float EnemiesNearTargetRange;
        private float EnemiesNearMeRange;
        private float InterruptibleEnemiesRange;

        private static IWoWUnit Cache(WoWUnit unit) => new CachedWoWUnit(unit);
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
            WoWLocalPlayer player;
            IWoWUnit cachedTarget, cachedPet, cachedPlayer;
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
            var listGroupMember = new List<IWoWUnit>();

            var targetPosition = cachedTarget.PositionWithoutType;
            var targetGuid = cachedTarget.Guid;
            var playerPosition = cachedPlayer.PositionWithoutType;
            var playerGuid = cachedPlayer.Guid;

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
                if(unit.IsPartyMember)
                {
                    listGroupMember.Add(cachedUnit);
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

                if (targetPosition.DistanceTo(unitPosition) <= EnemiesNearTargetRange && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyUnitsNearTarget.Add(cachedUnit);
                }

                if (unit.IsTargetingPartyMember && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyAttackingGroup.Add(cachedUnit);
                }

                var playerDistance = playerPosition.DistanceTo(unitPosition);
                if (playerDistance <= EnemiesNearMeRange && Reachable(playerPosition, unitPosition, ref cachedReachable))
                {
                    enemyUnitsNearPlayer.Add(cachedUnit);
                }

                if (playerDistance <= InterruptibleEnemiesRange && Reachable(playerPosition, unitPosition, ref cachedReachable) && unit.CanInterruptCasting)
                {
                    interruptibleEnemyUnits.Add(cachedUnit);
                }
            }

            Me = cachedPlayer;
            Target = cachedTarget;
            Pet = cachedPet;

            EnemyUnitsNearTarget = enemyUnitsNearTarget.ToArray();
            EnemyUnitsNearPlayer = enemyUnitsNearPlayer.ToArray();
            InterruptibleEnemyUnits = interruptibleEnemyUnits.ToArray();
            EnemyUnitsTargetingPlayer = enemyUnitsTargetingPlayer.ToArray();
            EnemyUnitsLootable = enemyUnitsLootable.ToArray();
            EnemyAttackingGroup = enemyAttackingGroup.ToArray();
            ListGroupMember = listGroupMember.ToArray();
        }

    }
}
