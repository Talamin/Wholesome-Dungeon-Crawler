using robotManager.Helpful;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Managers
{
    internal class AvoidAOEManager : IAvoidAOEManager
    {
        private readonly IEntityCache _entityCache;
        private readonly ICache _cache;
        private List<DangerZone> _dangerZones = new List<DangerZone>();
        private LFGRoles _myRole = CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole;

        private bool IAmInDangerZone => _dangerZones.Any(dangerZone => dangerZone.PositionInDangerZone(_entityCache.Me.PositionWT));
        public RepositionInfo RepositionInfo { get; private set; }

        public AvoidAOEManager(
            IEntityCache entityCache,
            ICache cache)
        {
            _entityCache = entityCache;
            _cache = cache;

            // We load the AOEs in a dictionary to speed up lookup
            foreach (KnownAOE knownAOE in _knownAOEs)
            {
                if (!knownAOE.AffectedRoles.Contains(_myRole)) continue;

                if (!_knowAOEsDic.ContainsKey(knownAOE.Id))
                {
                    _knowAOEsDic.Add(knownAOE.Id, knownAOE);
                }
                else
                {
                    Logger.LogError($"Multiple entries for AOE : {knownAOE.Id}");
                }
            }

            // We load the Forced Safe Zones in a dictionary to speed up lookup
            foreach (ForcedSafeZone forcedSafeZone in _knownForcedSafeZones)
            {
                if (!_forcedSafeZonesDic.ContainsKey(forcedSafeZone.BossId))
                {
                    _forcedSafeZonesDic.Add(forcedSafeZone.BossId, forcedSafeZone);
                }
                else
                {
                    Logger.LogError($"Multiple entries for Forced Safe Zone : {forcedSafeZone.BossId}");
                }
            }
        }

        public void Initialize()
        {
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            FightEvents.OnFightStart += FightEventHandler;
            FightEvents.OnFightLoop += FightEventHandler;
            MovementEvents.OnMovementPulse += MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse += MovementsEventsOnMoveToPulse;
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            FightEvents.OnFightStart -= FightEventHandler;
            FightEvents.OnFightLoop -= FightEventHandler;
            MovementEvents.OnMovementPulse -= MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse -= MovementsEventsOnMoveToPulse;
        }

        private void AddDangerZone(WoWObject wowObject, float radius)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == wowObject.Guid && dangerZone.Position.DistanceTo(wowObject.Position) < 1f))
            {
                return;
            }

            RemoveDangerZone(wowObject.Guid);
            _dangerZones.Add(new DangerZone(wowObject, radius));
        }

        private void RemoveDangerZone(ulong objectGuid)
        {
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid);
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            Vector3 myPos = _entityCache.Me.PositionWT;

            List<WoWObject> objectList = ObjectManager.ObjectList.ToList();

            // Clear danger zone if its corresponding object doesn't exist anymore in the OM
            List<ulong> dangerZonesToRemove = _dangerZones
                .Where(dangerZone => !objectList.Exists(wObject => wObject.Guid == dangerZone.Guid))
                .Select(dangerZone => dangerZone.Guid)
                .ToList();
            foreach (ulong dzToRemoveGuid in dangerZonesToRemove)
            {
                RemoveDangerZone(dzToRemoveGuid);
            }

            // Record danger zones
            foreach (WoWObject wowObject in objectList)
            {
                if (_knowAOEsDic.TryGetValue(wowObject.Entry, out KnownAOE knownAOE))
                {
                    switch (wowObject.Type)
                    {
                        case WoWObjectType.Unit:
                            WoWUnit unit = wowObject as WoWUnit;
                            if (unit.IsAlive)
                            {
                                AddDangerZone(wowObject, knownAOE.Radius);
                            }
                            else
                            {
                                RemoveDangerZone(unit.Guid);
                            }
                            break;
                        case WoWObjectType.DynamicObject:
                            DynamicObject dObject = new DynamicObject(wowObject.GetBaseAddress);
                            AddDangerZone(dObject, knownAOE.Radius);
                            break;
                        case WoWObjectType.GameObject:
                            AddDangerZone(wowObject, knownAOE.Radius);
                            break;
                        default:
                            break;
                    }
                }
            }

            // Is current fight a Forced Safe Zone fight?
            ForcedSafeZone forcedSafeZone = null;
            foreach (IWoWUnit enemy in _entityCache.EnemiesAttackingGroup)
            {
                if (_forcedSafeZonesDic.ContainsKey(enemy.Entry))
                {
                    forcedSafeZone = _forcedSafeZonesDic[enemy.Entry];
                    break;
                }
            }

            DangerZone currentDangerZone = _dangerZones.Find(dangerZone => dangerZone.PositionInDangerZone(myPos));
            bool inSafeZone = forcedSafeZone == null || forcedSafeZone.PositionInSafeZone(myPos);
            if (currentDangerZone != null || !inSafeZone)
            {
                RepositionInfo = new RepositionInfo(_dangerZones, forcedSafeZone, currentDangerZone, inSafeZone);
            }
            else
            {
                RepositionInfo = null;
            }
        }

        private void MovementEventsOnMovementPulse(List<Vector3> path, CancelEventArgs cancelable)
        {
            if (path == null || path.Count <= 0) return;

            // Don't cancel during pull
            if (Fight.InFight
                && _entityCache.Target != null
                && !_entityCache.Target.WowUnit.InCombat) return;

            for (int i = 0; i < path.Count - 1; i++)
            {
                DangerZone dangerZoneOnTheWay = _dangerZones
                    .FirstOrDefault(dz => 
                        dz.Position.DistanceTo(_entityCache.Me.PositionWT) < dz.Radius + 5
                        && WTPathFinder.PointDistanceToLine(path[i], path[i + 1], dz.Position) < dz.Radius);
                if (dangerZoneOnTheWay != null && !IAmInDangerZone)
                {
                    Logger.LogOnce($"Can't move, {dangerZoneOnTheWay.Name} is on the path. Waiting despawn.");
                    cancelable.Cancel = true;
                    return;
                }
            }
        }

        private void MovementsEventsOnMoveToPulse(Vector3 node, CancelEventArgs cancelable)
        {
            // Don't cancel during pull
            if (Fight.InFight
                && _entityCache.Target != null
                && !_entityCache.Target.WowUnit.InCombat) return;

            // Cancel moves into danger zones
            DangerZone dangerZoneOnTheWay = _dangerZones.FirstOrDefault(dz =>
                dz.Position.DistanceTo(_entityCache.Me.PositionWT) < dz.Radius + 5
                && dz.PositionInDangerZone(node));
            if (dangerZoneOnTheWay != null && !IAmInDangerZone)
            {
                Logger.LogOnce($"Can't move, {dangerZoneOnTheWay.Name} is on the next node. Waiting despawn.");
                cancelable.Cancel = true;
                return;
            }
        }

        private void FightEventHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (RepositionInfo != null)
            {
                Logger.LogOnce($"Canceled fight because we need to reposition");
                Lua.LuaDoString("SpellStopCasting();");
                cancelable.Cancel = true;
            }
        }

        private Dictionary<int, ForcedSafeZone> _forcedSafeZonesDic = new Dictionary<int, ForcedSafeZone>();
        private readonly List<ForcedSafeZone> _knownForcedSafeZones = new List<ForcedSafeZone>()
        { 
            // Shirrak fight - Auchenai Crypt
            new ForcedSafeZone(18371, new Vector3(-51.94074, -163.6697, 26.36175, "None"), 40),
        };

        private Dictionary<int, KnownAOE> _knowAOEsDic = new Dictionary<int, KnownAOE>();
        private static readonly List<LFGRoles> _everyone = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _meleeOnly = new List<LFGRoles>() { LFGRoles.Tank, LFGRoles.MDPS };
        private static readonly List<LFGRoles> _rangedOnly = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.RDPS };
        private static readonly List<LFGRoles> _everyoneExceptTank = new List<LFGRoles>() { LFGRoles.Heal, LFGRoles.MDPS, LFGRoles.RDPS };
        private readonly List<KnownAOE> _knownAOEs = new List<KnownAOE>()
        { 
            // Creeping Sludge (Foulspore Caverns)
            new KnownAOE(12222, 8f, _everyone),
            // Noxious Slime gas (Foulspore Caverns)
            new KnownAOE(21070, 8f, _everyone),
            // Proximity Mine (The Blood Furnace)
            new KnownAOE(181877, 8f, _everyoneExceptTank),
            // Liquid Fire (Hellfire Ramparts, last boss)   
            new KnownAOE(181890, 8f, _everyone),
            // Broggok poison cloud (Blood Furnace)
            new KnownAOE(17662, 15f, _everyone),
            // Underbog Mushroom (Underbog, Hungarfen boss)
            new KnownAOE(17990, 10f, _everyone),
            // Focus Target Visual (Mana Tombs, Shirrak the Dead Watcher)
            new KnownAOE(32286, 16f, _everyone),
            // Shirrak the Dead Watcher (cast debuff when close)
            new KnownAOE(18371, 15f, _rangedOnly),
            // Lightning Cloud (Hydromancer Thepias)
            new KnownAOE(25033, 12f, _everyone),
            // Arcane Sphere (Kael'thas Sunstrider)
            new KnownAOE(24708, 20f, _everyone),
            // Flame Strike (Kael'thas Sunstrider)
            new KnownAOE(24666, 10f, _everyone),
            // Axe Ingvar the Plunderer (Utgarde Keep)
            new KnownAOE(23997, 8f, _everyone),
            // Cloud of Disease (Scholomance)
            new KnownAOE(17742, 8f, _everyone),
            // Impale - Anub Arak (Azjol Nerub)
            new KnownAOE(29184, 5f, _everyone),
        };
    }
}
