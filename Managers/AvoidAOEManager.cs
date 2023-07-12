using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager.Wow.Patchables;

namespace WholesomeDungeonCrawler.Managers
{
    internal class AvoidAOEManager : IAvoidAOEManager
    {
        private readonly IEntityCache _entityCache;
        private readonly ICache _cache;

        private List<DangerZone> _dangerZones = new List<DangerZone>();
        //private List<Vector3> _safeSpots = new List<Vector3>();
        private List<Vector3> _escapePath;
        //private Vector3 _safeSpot = null;
        //private DangerZone _dangerZoneToEscape = null;
        //private List<(ulong guid, Vector3 position)> _blackListCache = new List<(ulong, Vector3)>();
        private float _marginRadius = 4f;
        private LFGRoles _myRole = CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole;

        public bool MustEscapeAOE => _dangerZones.Exists(dangerZone => dangerZone.Position.DistanceTo(_entityCache.Me.PositionWithoutType) < dangerZone.Radius);
        public List<Vector3> GetEscapePath => _escapePath;

        public AvoidAOEManager(
            IEntityCache entityCache,
            ICache cache)
        {
            _entityCache = entityCache;
            _cache = cache;

            // We load the AOEs in a dictionary to speed up AOE lookup
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
        }

        public void Initialize()
        {
            Radar3D.OnDrawEvent += Radar3DOnDrawEvent;
            Radar3D.Pulse();
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            MovementEvents.OnMovementPulse += MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse += MovementsEventsOnMoveToPulse;
            FightEvents.OnFightStart += FightEventsOnFightStart;
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= Radar3DOnDrawEvent;
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            MovementEvents.OnMovementPulse -= MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse -= MovementsEventsOnMoveToPulse;
            FightEvents.OnFightStart -= FightEventsOnFightStart;
        }

        private void AddDangerZone(WoWObject wowObject, float radius)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == wowObject.Guid && dangerZone.Position.DistanceTo(wowObject.Position) < 1f))
            {
                return;
            }

            RemoveDangerZone(wowObject.Guid);
            _dangerZones.Add(new DangerZone(wowObject, radius));
            //wManagerSetting.AddBlackListZone(wowObject.Position, radius, (ContinentId)Usefuls.ContinentId, isSessionBlacklist: true);
            //_blackListCache.Add((wowObject.Guid, wowObject.Position));
        }

        private void RemoveDangerZone(ulong objectGuid)
        {
            /*
            if (_blackListCache.Exists(bl => bl.guid == objectGuid))
            {
                (ulong guid, Vector3 position) _cachedBl = _blackListCache.Find(bl => bl.guid == objectGuid);
                wManagerSetting.GetListZoneBlackListed().RemoveAll(blZone => blZone.GetPosition() == _cachedBl.position);
                _blackListCache.RemoveAll(bl => bl.guid == objectGuid);
            }
            */
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid);
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            //bool automaticallyDetectRange = false;

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

            foreach (WoWObject wowObject in objectList)
            {

                // Uncomment this part to detect objects in the log
                /*
                if (wowObject.Type == WoWObjectType.DynamicObject)
                {
                    DynamicObject dObject = new DynamicObject(wowObject.GetBaseAddress);
                    Logger.LogOnce($"DYNAMIC: {dObject.Name} -> {dObject.Entry}", true);
                }
                
                if (wowObject.Type != WoWObjectType.Unit
                    && wowObject.Type != WoWObjectType.DynamicObject
                    && wowObject.Type != WoWObjectType.GameObject)
                {
                    Logger.LogOnce($"WoWOBJECT: {wowObject.Name} -> {wowObject.Entry}", true);
                }
                */
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
                            //knownAOE = automaticallyDetectRange ? dObject.Radius : knownAOE;
                            AddDangerZone(dObject, knownAOE.Radius);
                            break;
                        case WoWObjectType.GameObject:
                            AddDangerZone(wowObject, knownAOE.Radius);
                            break;
                        default:
                            Logger.LogError($"Invalid object type {wowObject.Type} for object with entry {wowObject.Entry} in AoE avoider.");
                            break;
                    }
                }
            }

            // In danger zone
            DangerZone dangerZoneToEscape = _dangerZones
                .Find(dangerZone => dangerZone.Position.DistanceTo(myPos) < dangerZone.Radius);
            if (dangerZoneToEscape != null)
            {
                if (!MovementManager.InMovement)
                {
                    Logger.Log($"Trying to escape {dangerZoneToEscape.Name}");
                    List<Vector3> safeSpots = new List<Vector3>();
                    for (byte range = 10; range <= 50; range += 5)
                    {
                        for (int y = -range; y <= range; y += 5)
                        {
                            for (int x = -range; x <= range; x += 5)
                            {
                                Vector3 safePosition = myPos + new Vector3(x, y, _entityCache.Me.PositionWithoutType.Z);
                                if (!_dangerZones.Any(dangerZone => dangerZone.Position.DistanceTo2D(safePosition) < dangerZone.Radius + _marginRadius)
                                    && !_entityCache.EnemyUnitsList.Any(enemy => enemy.TargetGuid <= 0 && safePosition.DistanceTo(enemy.PositionWithoutType) < 30)) // don't go towards unpulled enemies
                                {
                                    safeSpots.Add(safePosition);
                                }
                            }
                        }
                    }

                    if (safeSpots.Count <= 0)
                    {
                        Logger.LogError("Failed to find any safe spot!");
                        return;
                    }

                    // Prefer a position nearby myself
                    IWoWPlayer tank = _entityCache.TankUnit;
                    Vector3 positionPreferred = _entityCache.Me.PositionWithoutType;
                    List<Vector3> closestSpotsFromPreferred = safeSpots
                        .OrderBy(spot => positionPreferred.DistanceTo(spot))
                        .ToList();
                    foreach (Vector3 spot in closestSpotsFromPreferred)
                    {
                        Vector3 spotPosition = new Vector3(spot.X, spot.Y, PathFinder.GetZPosition(spot));

                        // Should always be in range of tank
                        if (tank != null && tank.PositionWithoutType.DistanceTo(spotPosition) > 27)
                        {
                            continue;
                        }

                        if (!TraceLine.TraceLineGo(spotPosition))
                        {
                            List<Vector3> pathToSafeSpot = PathFinder.FindPath(spotPosition, out bool foundPath);
                            float straightLineDistance = myPos.DistanceTo(spotPosition);

                            if (foundPath)
                            {
                                // Avoid big detours or fall off cliffs
                                if (WTPathFinder.CalculatePathTotalDistance(pathToSafeSpot) > straightLineDistance * 1.8)
                                {
                                    continue;
                                }

                                if (Fight.InFight)
                                {
                                    Lua.LuaDoString("SpellStopCasting();");
                                    Fight.StopFight();
                                }

                                _escapePath = pathToSafeSpot;
                                return;
                            }
                        }
                    }
                    Logger.LogError($"No escape route found!");
                }
            }
            else
            {
                _escapePath = null;
            }
        }

        private void MovementEventsOnMovementPulse(List<Vector3> path, CancelEventArgs cancelable)
        {
            if (path == null || path.Count <= 0) return;
            // Don't cancel during pull
            if (Fight.InFight && _entityCache.Target.TargetGuid <= 0) return;

            // Cancel moves into danger zones
            DangerZone dangerZoneOnTheWay = _dangerZones.Where(dangerZone =>
                dangerZone.Position.DistanceTo(path.Last()) < dangerZone.Radius
                && _entityCache.Me.PositionWithoutType.DistanceTo(path.Last()) < dangerZone.Radius + _marginRadius + 4)
                .FirstOrDefault();
            if (dangerZoneOnTheWay != null)
            {
                Logger.LogOnce($"Can't move, {dangerZoneOnTheWay.Name} is on the way. Waiting despawn.");
                cancelable.Cancel = true;
                return;
            }

            // Never cancel escape
            if (MustEscapeAOE)
            {
                if (_escapePath?.Last() == path.Last())
                {
                    return;
                }
                else
                {
                    cancelable.Cancel = true;
                    return;
                }
            }
        }

        private void MovementsEventsOnMoveToPulse(Vector3 node, CancelEventArgs cancelable)
        {
            // Don't cancel during pull
            if (Fight.InFight && _entityCache.Target.TargetGuid <= 0) return;

            // Never cancel escape
            if (MustEscapeAOE)
            {
                if (_escapePath != null && _escapePath.Contains(node))
                {
                    return;
                }
                else
                {
                    // Cancel everything else
                    cancelable.Cancel = true;
                    return;
                }
            }

            // Cancel moves into danger zones
            DangerZone dangerZoneOnTheWay = _dangerZones.Where(dangerZone =>
                dangerZone.Position.DistanceTo(node) < dangerZone.Radius
                && _entityCache.Me.PositionWithoutType.DistanceTo(node) < dangerZone.Radius + _marginRadius + 4)
                .FirstOrDefault();
            if (dangerZoneOnTheWay != null)
            {
                Logger.LogOnce($"Can't move, {dangerZoneOnTheWay.Name} is on the way. Waiting despawn.");
                cancelable.Cancel = true;
                return;
            }
        }

        private void FightEventsOnFightStart(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (MustEscapeAOE)
            {
                cancelable.Cancel = true;
            }
        }

        private void Radar3DOnDrawEvent()
        {
            foreach (DangerZone dangerZone in _dangerZones)
            {
                Radar3D.DrawCircle(dangerZone.Position, dangerZone.Radius, Color.IndianRed, false, 150);
            }
            /*
            foreach (Vector3 safeSpot in _safeSpots)
            {
                Radar3D.DrawCircle(safeSpot, 0.2f, Color.CadetBlue, true, 200);
            }

            if (_safeSpot != null)
            {
                Radar3D.DrawCircle(_safeSpot, 0.4f, Color.ForestGreen, false, 200);
            }
            
            for (int i = 0; i < _escapePath.Count - 1; i++)
            {
                Radar3D.DrawLine(_escapePath[i], _escapePath[i + 1], Color.ForestGreen);
            }
            */
        }

        public class DangerZone
        {
            public Vector3 Position { get; private set; }
            public float Radius { get; private set; }
            public ulong Guid { get; private set; }
            public string Name { get; private set; }
            public WoWObjectType ObjectType { get; private set; }

            public DangerZone(WoWObject wowObject, float radius)
            {
                Position = wowObject.Position;
                Guid = wowObject.Guid;
                Name = string.IsNullOrEmpty(wowObject.Name) ? "Unknown object" : wowObject.Name;
                ObjectType = wowObject.Type;
                Radius = radius;
            }
        }

        private struct KnownAOE
        {
            public int Id { get; private set; }
            public float Radius { get; private set; }
            public List<LFGRoles> AffectedRoles { get; private set; }
            public Func<bool> Condition { get; private set; }
            public bool IsConditionMet => Condition == null || Condition();

            public KnownAOE(int id, float radius, List<LFGRoles> affectedRoles, Func<bool> condition = null)
            {
                Id = id;
                Radius = radius;
                AffectedRoles = affectedRoles;
                Condition = condition;
            }
        }

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
            new KnownAOE(181877, 8f, _everyone),
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
        };
        /*
        private readonly Dictionary<int, float> _knowAOEs = new Dictionary<int, float> // { Entry, Radius }
        {
            // 5 man
            //{8317, 8f}, // Atal'ai Deathwalker's Spirit (Temple)
            {12222, 8f}, // Creeping Sludge (Foulspore Caverns)
            {21070, 8f}, // Noxious Slime gas (Foulspore Caverns)
            {181877, 8f},// Proximity Mine (The Blood Furnace)
            {181890, 8f}, // Liquid Fire (Hellfire Ramparts, last boss)
            {17662, 15f}, // Broggok poison cloud (Blood Furnace)
            {17990, 10f }, // Underbog Mushroom (Underbog, Hungarfen boss)
            {32286, 10f }, // Focus Target Visual (Mana Tombs, Shirrak the Dead Watcher)

            {42748, 4f}, // Shadow Axe (Ingvar The Plunderer)
            {47958, 4f}, // Crystal Spikes (Ormorok The Tree Shaper)
            {47579, 4f}, // Freezing Cloud (Skadi)
            {60020, 4f}, // Freezing Cloud (Skadi)
            {48381, 4f}, // Spirit Fount (King Ymiron)
            {59321, 4f}, // Spirit Fount (King Ymiron)
            {51103, 4f}, // Frostbomb (Mage-Lord Urom)
            {56926, 4f}, // Thundershock (Jedoga Shadowseeker)
            {60029, 4f}, // Thundershock (Jedoga Shadowseeker - Heroic)
            {55847, 4f}, // Shadow Void (Risen Drakkari Soulmage - Drak'Tharon Keep - Normal)
            {59014, 4f}, // Shadow Void (Risen Drakkari Soulmage - Drak'Tharon Keep - Normal)
            {49548, 4f}, // Poison Cloud (The Prophet Tharon'ja - Drak'Tharon Keep - Normal)
            {59969, 4f}, // Poison Cloud (The Prophet Tharon'ja - Drak'Tharon Keep - Heroic)
            {49034, 4f}, // Blizzard (Novos the Summoner - Drak'Tharon Keep - Heroic)
            {47346, 4f}, // Arcane Field (Novos the Summoner - Drak'Tharon Keep - Heroic)            
            {57061, 4f}, // Poison Cloud (Poisonous Mushroom - Old Kingdom)
            {59116, 4f}, // Poison Cloud (Savage Cave Beast - Old Kingdom)
            {56867, 4f}, // Poison Cloud (Savage Cave Beast - Old Kingdom)            
            {53400, 4f}, // Acid Cloud (Hadronox)
            {59419, 4f}, // Acid Cloud (Hadronox - Heroic)            
            {50752, 4f}, // Storm of Grief (Maiden of Grief)
            {59772, 4f}, // Storm of Grief (Maiden of Grief - Heroic)
            {50915, 4f}, // Raging Consecration (High General Abbendis - Dragonblight)
            {59451, 4f}, // Mojo Puddle
            {58994, 4f}, // Mojo Puddle - Heroic
            {55627, 4f}, // Mojo Puddle
            {62466, 4f}, // Lightning Charge (Thorim)
            {36536, 4f },// Well of  Souls
            {68820, 4f}, // Well Of Souls (Devourer of Souls - Forge of Souls)
            {68863, 4f}, // Well Of Souls (Devourer of Souls - Forge of Souls - Heroic)

            //Raid
            {62548, 4f}, // Scorch (Ignis-10)
            {62549, 4f}, // Scorch (Ignis-10)
            {63475, 4f}, // Scorch (Ignis-25)
            {63476, 4f}, // Scorch (Ignis-25)
            {29371, 4f}, // Eruption (Heigan the Unclean)
            {58965, 4f}, // Choking Cloud (Archavon 10)
            {61672, 4f}, // Choking Cloud (Archavon 25)
            {28547, 4f}, // Chill (Sapphron 10)
            {55699, 4f}, // Chill (Sapphron 25)
            {60919, 4f}, // Rock Shower (Archavon Warder 10)
            {60923, 4f}, // Rock Shower (Archavon Warder 25)
            {54362, 4f}, // Poison (Grobbulus)
            {28158, 4f}, // Poison (Grobbulus)
            {28241, 4f}, // Poison (Grobbulus)
            {54363, 4f}, // Poison (Grobbulus)            
            {64851, 4f}, // Flaming Rune (Ulduar trash)
            {64989, 4f}, // Flaming Rune (Ulduar trash)
            {62451, 4f}, // Unstable Energy (Freya 10)
            {62865, 4f}, // Unstable Energy (Freya 25)
            {66881, 4f}, // Slime Pool (Northrend Beasts 10 Normal)
            {67639, 4f}, // Slime Pool (Northrend Beasts 10 Heroic)
            {67638, 4f}, // Slime Pool (Northrend Beasts 25 Normal)
            {67640, 4f}, // Slime Pool (Northrend Beasts 25 Heroic)
            {66320, 4f}, // Fire Bomb (Northrend Beasts 10 Normal)
            {67473, 4f}, // Fire Bomb (Northrend Beasts 10 Heroic)
            {67472, 4f}, // Fire Bomb (Northrend Beasts 25 Normal)
            {67475, 4f}, // Fire Bomb (Northrend Beasts 25 Heroic)
            {66877, 4f}, // Legion Flame (Lord Jaraxxus 10 Normal)
            {67071, 4f}, // Legion Flame (Lord Jaraxxus 10 Heroic)
            {67070, 4f}, // Legion Flame (Lord Jaraxxus 25 Normal)
            {67072, 4f}, // Legion Flame (Lord Jaraxxus 25 Heroic)

            // Fails (might not be able to avoid)            
            {57581, 4f }, // Void Blast (OS 10)
            {59128, 4f }, // Void Blast (OS 25)
            {27812, 4f }, // Flame Tsunami (OS)
            {71944, 4f }, // Shock Vortex (Prince Valanar 10)
            {72812, 4f }, // Shock Vortex (Prince Valanar 25)
            {72813, 4f }, // Shock Vortex (Prince Valanar 10H)
            {72814, 4f }, // Shock Vortex (Prince Valanar 25H)
            {70853, 4f }, // Malleable Goo (Professor Putricide 10)
            {72458, 4f }, // Malleable Goo (Professor Putricide 25)
            {72873, 4f }, // Malleable Goo (Professor Putricide 10 Heroic)
            {72874, 4f }, // Malleable Goo (Professor Putricide 25 Heroic)
            {71279, 4f }, // Choking Gas Explosion (Professor Putricide 10)
            {72459, 4f }, // Choking Gas Explosion (Professor Putricide 25)
            {72621, 4f }, // Choking Gas Explosion (Professor Putricide 10H)
            {72622, 4f }, // Choking Gas Explosion (Professor Putricide 25H)
            {70702, 4f }, // Column of Frost (Valithria Dreamwalker - 10)
            {71746, 4f }, // Column of Frost (Valithria Dreamwalker - 25)
            {70744, 4f }, // Acid Burst (Valithria Dreamwalker - 10)
            {71733, 4f }, // Acid Burst (Valithria Dreamwalker - 25)
            {71077, 4f }, // Tail Smash (Sindragosa)
            {71053, 4f }, // Frost Bomb (Sindragosa)
            {70123, 4f }, // Blistering Cold (Sindragosa - 10)
            {71047, 4f }, // Blistering Cold (Sindragosa - 25)
            {71048, 4f }, // Blistering Cold (Sindragosa - 10H)
            {71049, 4f }, // Blistering Cold (Sindragosa - 25H)
            {70503, 4f }, // Spirit Burst (Lich King - 10)
            {73806, 4f }, // Spirit Burst (Lich King - 25)
            {73807, 4f }, // Spirit Burst (Lich King - 10H)
            {73808, 4f }, // Spirit Burst (Lich King - 25H)
            {66351, 4f }, // Mine Explosion (Mimiron 10)
            {63009, 4f }, // Mine Explosion (Mimiron 25)
            {69680, 4f }, // Gunship Explosion (Gunship Battle - 10 Normal)
            {69687, 4f }, // Gunship Explosion (Gunship Battle - 25 Normal)
            {69688, 4f }, // Gunship Explosion (Gunship Battle - 10 Heroic)
            {69689, 4f }, // Gunship Explosion (Gunship Battle - 25 Heroic)
            {69019, 4f }, // Explosive Barrage (Ick - Normal)
            {70433, 4f }, // Explosive Barrage (Ick - Heroic)
            {71377, 4f }, // Icy Blast (Rimefang - ICC 10)
            {71378, 4f }, // Icy Blast (Rimefang - ICC 25)
            {71544, 4f }, // Vengeful Blast (Lady Deathwhisper 10)
            {72010, 4f }, // Vengeful Blast (Lady Deathwhisper 25)
            {72011, 4f }, // Vengeful Blast (Lady Deathwhisper 10H)
            {72012, 4f }, // Vengeful Blast (Lady Deathwhisper 25H)
            {72549, 4f }, // Malleable Goo (Festergut 10H)	
            {72550, 4f }, // Malleable Goo (Festergut 25H)	
            {69108, 4f }, // Ice Burst (Lich King 10)
            {73773, 4f }, // Ice Burst (Lich King 25)
            {73774, 4f }, // Ice Burst (Lich King 10H)
            {73775, 4f }, // Ice Burst (Lich King 25H)
            {73529, 4f }, // Shadow Trap (Lich King - Heroic)
            {74648, 4f }, // Meteor Strike (Halion 10)
            {75877, 4f }, // Meteor Strike (Halion 25)
            {75878, 4f }, // Meteor Strike (Halion 10H)
            {75879, 4f }, // Meteor Strike (Halion 25H)
            {74769, 4f }, // Twilight Cutter (Halion 10)
            {77844, 4f }, // Twilight Cutter (Halion 25)
            {77845, 4f }, // Twilight Cutter (Halion 10H)
            {77846, 4f }, // Twilight Cutter (Halion 25H)
            {69055, 4f }, // Bone Slice (Lord Marrowgar 10)
            {70814, 4f }, // Bone Slice (Lord Marrowgar 25)
            {70633, 4f }, // Gut Spray (Valithria Dreamwalker 10)
            {71283, 4f }, // Gut Spray (Valithria Dreamwalker 25)
            {72025, 4f }, // Gut Spray (Valithria Dreamwalker 10H)
            {72026, 4f }, // Gut Spray (Valithria Dreamwalker 25H)
            {69242, 4f }, // Soul Shriek (Lich King 10)
            {73800, 4f }, // Soul Shriek (Lich King 25)
            {73801, 4f }, // Soul Shriek (Lich King 10H)
            {73802, 4f }, // Soul Shriek (Lich King 25H)
            {72149, 4f }, // Shockwave (Lich King 10)
            {73794, 4f }, // Shockwave (Lich King 25)
            {73795, 4f }, // Shockwave (Lich King 10H)
            {73796, 4f }, // Shockwave (Lich King 25H)
            {74403, 4f }, // Flame Breath (Saviana Ragefire 10)
            {74404, 4f }, // Flame Breath (Saviana Ragefire 25)
            {74524, 4f }, // Cleave (Halion)
            {74525, 4f }, // Flame Breath (Halion 10)
            {74526, 4f }, // Flame Breath (Halion 10H)
            {74527, 4f }, // Flame Breath (Halion 25)
            {74528, 4f }, // Flame Breath (Halion 25H)
            {74806, 4f }, // Dark Breath (Halion 10)
            {75954, 4f }, // Dark Breath (Halion 10H)
            {75955, 4f }, // Dark Breath (Halion 25)
            {75956, 4f }, // Dark Breath (Halion 25H)
            {74531, 4f }, // Tail Lash (Halion)
            {75418, 4f }, // Shockwave (Charscale Assaulter)
            {69492, 4f }, // Shadow Cleave (Deathspeaker Zealot, ICC)
            {70670, 4f }, // Shadow Cleave (Lady Deathwhisper 10)
            {72006, 4f }, // Shadow Cleave (Lady Deathwhisper 25)
            {72493, 4f }, // Shadow Cleave (Lady Deathwhisper 10H)
            {72494, 4f }, // Shadow Cleave (Lady Deathwhisper 25H)
            {70176, 4f }, // Vomit Spray (Lumbering Abomination, HoR)
            {70181, 4f }, // Vomit Spray (Lumbering Abomination, HoR Heroic)
            {71369, 4f }, // Tail Sweep (Spinestalker, ICC 10)
            {71370, 4f }, // Tail Sweep (Spinestalker, ICC 25)
            {71386, 4f }, // Frost Breath (Rimefang, ICC 25)
            {69649, 4f }, // Frost Breath P1 (Sindragosa, ICC 10)
            {73061, 4f }, // Frost Breath P2 (Sindragosa, ICC 10)
            {71056, 4f }, // Frost Breath P1 (Sindragosa, ICC 25)
            {73062, 4f }, // Frost Breath P2 (Sindragosa, ICC 25)
            {71057, 4f }, // Frost Breath P1 (Sindragosa, ICC 10H)
            {73063, 4f }, // Frost Breath P2 (Sindragosa, ICC 10H)
            {71058, 4f }, // Frost Breath P1 (Sindragosa, ICC 25H)
            {73064, 4f }, // Frost Breath P2 (Sindragosa, ICC 25H)
        };*/

        private class DynamicObject : WoWObject
        {
            public DynamicObject(uint address) : base(address) { }

            public override Vector3 Position =>
                new Vector3(Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xE8),
                    Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xEC),
                    Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xF0));
            public override string Name => new Spell(SpellID).Name;
            public override float GetDistance => Position.DistanceTo(ObjectManager.Me.PositionWithoutType);
            public ulong Caster =>
                Memory.WowMemory.Memory.ReadUInt64(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.Caster));
            public int SpellID =>
                Memory.WowMemory.Memory.ReadInt32(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.SpellID));
            public float Radius =>
                Memory.WowMemory.Memory.ReadFloat(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.Radius));
            public int CastTime =>
                Memory.WowMemory.Memory.ReadInt32(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.CastTime));
        }
    }
}
