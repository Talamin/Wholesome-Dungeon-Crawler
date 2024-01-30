using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;
using WholesomeDungeonCrawler.Managers.ManagedEvents;
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

        private Dictionary<int, KnownAOE> _knowAOEsDic = new Dictionary<int, KnownAOE>();

        private readonly SortedSet<int> _relevantBuffEnemyIds = new SortedSet<int>();
        private readonly SortedSet<int> _relevantDebuffEnemyIds = new SortedSet<int>();

        private readonly Dictionary<int, DangerSpell> _enemiesSpellsById = new Dictionary<int, DangerSpell>();
        private readonly Lookup<int, DangerBuff> _enemiesBuffsByUnit;
        private readonly Lookup<int, DangerDebuff> _enemiesDebuffsByUnit;

        private Dictionary<int, ForcedSafeZone> _forcedSafeZonesDic = new Dictionary<int, ForcedSafeZone>();

        private bool IAmInDangerZone => _dangerZones.Any(dangerZone => dangerZone.PositionInDangerZone(_entityCache.Me.PositionWT));
        public RepositionInfo RepositionInfo { get; private set; }

        public AvoidAOEManager(IEntityCache entityCache, ICache cache)
        {
            _entityCache = entityCache;
            _cache = cache;

            _enemiesBuffsByUnit = (Lookup<int, DangerBuff>)DangerList.GetEnemyBuffs
                .Where(eb => eb.AffectedRoles.Contains(_myRole))
                .ToLookup(eb => eb.UnitId, eb => eb);

            _enemiesDebuffsByUnit = (Lookup<int, DangerDebuff>)DangerList.GetEnemyDebuffs
                .Where(eb => eb.AffectedRoles.Contains(_myRole))
                .ToLookup(eb => eb.UnitId, eb => eb);

            _knowAOEsDic = DangerList.GetKnownAOEs
                .Where(kaoe => kaoe.AffectedRoles.Contains(_myRole))
                .ToDictionary(kaoe => kaoe.Id, kaoe => kaoe);

            _enemiesSpellsById = DangerList.GetEnemySpells
                .Where(es => es.AffectedRoles.Contains(_myRole))
                .ToDictionary(es => es.SpellId, es => es);

            _forcedSafeZonesDic = DangerList.GetForcedSafeZones
                .ToDictionary(es => es.BossId, es => es);

            foreach (DangerBuff eb in DangerList.GetEnemyBuffs)
            {
                _relevantBuffEnemyIds.Add(eb.UnitId);
            }

            foreach (DangerDebuff eb in DangerList.GetEnemyDebuffs)
            {
                _relevantDebuffEnemyIds.Add(eb.UnitId);
            }

        }

        public void Initialize()
        {
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            FightEvents.OnFightStart += FightEventHandler;
            FightEvents.OnFightLoop += FightEventHandler;
            MovementEvents.OnMovementPulse += MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse += MovementsEventsOnMoveToPulse;
            MovementEvents.OnMovementLoop += OnMovementLoop;
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
            {
                if (!Radar3D.IsLaunched) Radar3D.Pulse();
                Radar3D.OnDrawEvent += DrawEventAOE;
            }
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            FightEvents.OnFightStart -= FightEventHandler;
            FightEvents.OnFightLoop -= FightEventHandler;
            MovementEvents.OnMovementPulse -= MovementEventsOnMovementPulse;
            MovementEvents.OnMoveToPulse -= MovementsEventsOnMoveToPulse;
            MovementEvents.OnMovementLoop -= OnMovementLoop;
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
                Radar3D.OnDrawEvent -= DrawEventAOE;
            Radar3D.Stop();
        }

        public bool CheckSpells(List<string> args)
        {
            if (int.TryParse(args[8], out int spellId)
                && _enemiesSpellsById.ContainsKey(spellId))
            {
                ulong unitGuid = (ulong)Convert.ToInt64(args[2], 16);
                DangerSpell enemySpell = _enemiesSpellsById[spellId];
                ICachedWoWUnit enemy = _entityCache.EnemyUnitsList.FirstOrDefault(e => e.Guid == unitGuid);
                if (enemy != null)
                {
                    AddSpellDangerZone(enemy, enemySpell, args[9]);
                    CalculateReposition();
                    return true;
                }
            }
            return false;
        }

        private void AddObjectDangerZone(WoWObject wowObject, KnownAOE knownAOE)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == wowObject.Guid && dangerZone.Type == DangerType.GameObject && dangerZone.Position.DistanceTo(wowObject.Position) < 1f))
            {
                return;
            }
            RemoveAllObjectDangerZones(wowObject.Guid);
            DangerObject dangerObject = new DangerObject(wowObject, knownAOE);
            _dangerZones.Add(new DangerZone(dangerObject));
        }

        private void AddSpellDangerZone(ICachedWoWUnit unit, DangerSpell spell, string spellName)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == unit.Guid && dangerZone.Danger.Equals(spell)))
            {
                return;
            }
            _dangerZones.Add(new DangerZone(unit, spell, spellName));
        }

        private void AddBuffDangerZone(ICachedWoWUnit unit, DangerBuff buff, float duration)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == unit.Guid && dangerZone.Danger.Equals(buff)))
            {
                return;
            }
            _dangerZones.Add(new DangerZone(unit, buff, duration));
        }

        private void AddDebuffDangerZone(ICachedWoWUnit unit, DangerDebuff buff, float duration)
        {
            if (_dangerZones.Any(dangerZone => dangerZone.Guid == unit.Guid && dangerZone.Danger.Equals(buff)))
            {
                return;
            }
            _dangerZones.Add(new DangerZone(unit, buff, duration));
        }

        private void RemoveAllObjectDangerZones(ulong objectGuid)
        {
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid && dz.Type == DangerType.GameObject);
        }

        private void RemoveAllSpellDangerZones(ulong objectGuid)
        {
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid && dz.Type == DangerType.Spell);
        }

        private void RemoveAllBuffDangerZones(ulong objectGuid)
        {
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid && dz.Type == DangerType.Buff);
        }

        private void RemoveAllDebuffDangerZones(ulong objectGuid)
        {
            _dangerZones.RemoveAll(dz => dz.Guid == objectGuid && dz.Type == DangerType.Debuff);
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();

            List<WoWObject> objectList = ObjectManager.ObjectList;

           // Logger.Log($"OM has pulsed! Currently : {_dangerZones.Count} Zones under management.");

            // Clear danger zone if its corresponding object doesn't exist anymore in the OM
            List<ulong> dangerZonesToRemove = _dangerZones
                .Where(dangerZone => dangerZone.Type == DangerType.GameObject && !objectList.Exists(wObject => wObject.Guid == dangerZone.Guid))
                .Select(dangerZone => dangerZone.Guid)
                .ToList();
            foreach (ulong dzToRemoveGuid in dangerZonesToRemove)
            {
                RemoveAllObjectDangerZones(dzToRemoveGuid);
            }

            // Clear buff / debuff zones if their corresponding timers have expired or subject has moved
            List<ulong> expiredDangerBuffs = new List<ulong>();
            List<ulong> expiredDangerDebuffs = new List<ulong>();
            foreach (DangerZone dangerZone in _dangerZones)
            {
                if (dangerZone.Type == DangerType.Buff)
                {
                    if (dangerZone.Timer != null && dangerZone.Timer.IsReady)
                    {
                        expiredDangerBuffs.Add(dangerZone.Guid);
                    }                        
                    else
                    {
                        ICachedWoWUnit enemy = _entityCache.EnemyUnitsList.FirstOrDefault(e => e.Guid == dangerZone.Guid);
                        if (enemy == null || dangerZone.Position.DistanceTo(enemy.WowUnit.Position) > 1)
                        {
                            expiredDangerBuffs.Add(dangerZone.Guid);
                        }
                    }
                } 
                else if (dangerZone.Type == DangerType.Debuff)
                {
                    if (dangerZone.Timer != null && dangerZone.Timer.IsReady)
                    {
                        expiredDangerBuffs.Add(dangerZone.Guid);
                    }
                    else
                    {
                        ICachedWoWUnit unit = _entityCache.ListGroupMember.FirstOrDefault(u => u.Guid == dangerZone.Guid);
                        if (unit == null || dangerZone.Position.DistanceTo(unit.WowUnit.Position) > 1)
                        {
                            expiredDangerDebuffs.Add(dangerZone.Guid);
                        }
                    }
                }
            }
            foreach (ulong dzToRemoveGuid in expiredDangerBuffs)
            {
                RemoveAllBuffDangerZones(dzToRemoveGuid);
            }
            foreach (ulong dzToRemoveGuid in expiredDangerDebuffs)
            {
                RemoveAllDebuffDangerZones(dzToRemoveGuid);
            }

            // Clear Spell zones if their corresponding timers have expired
            List<ulong> expiredDangerZoneSpells = _dangerZones
                .Where(dangerZone => dangerZone.Type == DangerType.Spell && dangerZone.Timer != null && dangerZone.Timer.IsReady)
                .Select(dangerZone => dangerZone.Guid)
                .ToList();
            foreach (ulong dzToRemoveGuid in expiredDangerZoneSpells)
            {
                RemoveAllSpellDangerZones(dzToRemoveGuid);
            }

            // Add new buff zones
            foreach (ICachedWoWUnit unit in _entityCache.EnemyUnitsList)
            {
                if (_relevantBuffEnemyIds.Contains(unit.Entry))
                {
                    foreach (DangerBuff spell in _enemiesBuffsByUnit[unit.Entry])
                    {
                        if (unit.WowUnit.HaveBuff(spell.Name))
                        {
                            Logger.Log($"Creating buff danger: {spell.Name} for {unit.WowUnit.BuffTimeLeft(spell.Name)}s.");
                            AddBuffDangerZone(unit, spell, unit.WowUnit.BuffTimeLeft(spell.Name));
                        }
                    }
                }
            }
            // Add new debuff zones
            foreach (ICachedWoWUnit unit in _entityCache.EnemyUnitsList)
            {
                if (_relevantDebuffEnemyIds.Contains(unit.Entry))
                {
                    foreach (DangerDebuff debuff in _enemiesDebuffsByUnit[unit.Entry])
                    {
                        foreach (ICachedWoWPlayer player in _entityCache.ListGroupMember)
                        if (player.WowUnit.HaveBuff(debuff.Name))
                        {
                            Logger.Log($"Creating debuff danger: {debuff.Name} on {player.Name} for {player.WowUnit.BuffTimeLeft(debuff.Name)}s.");
                            AddDebuffDangerZone(player, debuff, player.WowUnit.BuffTimeLeft(debuff.Name));
                        }
                    }
                }
            }

            // Add new object zones
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
                                AddObjectDangerZone(wowObject, knownAOE);
                            }
                            else
                            {
                                RemoveAllObjectDangerZones(unit.Guid);
                            }
                            break;
                        case WoWObjectType.DynamicObject:
                            DynamicObject dObject = new DynamicObject(wowObject.GetBaseAddress);
                            AddObjectDangerZone(dObject, knownAOE);
                            break;
                        case WoWObjectType.GameObject:
                            AddObjectDangerZone(wowObject, knownAOE);
                            break;
                        default:
                            break;
                    }
                }
            }
            CalculateReposition();
        }

        private void CalculateReposition()
        {
            // Don't cancel escape
            if (RepositionInfo != null && MovementManager.InMovement) return;

            Vector3 myPos = _entityCache.Me.PositionWT;

            // Is current fight a Forced Safe Zone fight?
            ForcedSafeZone forcedSafeZone = null;
            foreach (ICachedWoWUnit enemy in _entityCache.EnemiesAttackingGroup)
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
                //Logger.Log($"Standing in danger zone!: {currentDangerZone.Name} - {currentDangerZone.Timer?.TimeLeft()}s.");
                RepositionInfo = new RepositionInfo(_dangerZones, forcedSafeZone, currentDangerZone, inSafeZone);
            }
            else
            {
                RepositionInfo = null;
            }
        }

        private void OnMovementLoop()
        {
            CheckPathForDangerZones(MovementManager.CurrentPath, null);
        }

        private void MovementEventsOnMovementPulse(List<Vector3> path, CancelEventArgs cancelable)
        {
            CheckPathForDangerZones(path, cancelable);
        }

        private void MovementsEventsOnMoveToPulse(Vector3 node, CancelEventArgs cancelable)
        {
            CheckPathForDangerZones(MovementManager.CurrentPath, cancelable);
        }

        private void CheckPathForDangerZones(List<Vector3> currentPath, CancelEventArgs cancelable)
        {
            // Don't cancel if fleeing
            if (RepositionInfo != null) return;

            List<DangerZone> dangerZones = new List<DangerZone>(_dangerZones);
            if (dangerZones.Count > 0)
            {
                List<Vector3> path = new List<Vector3>(currentPath);
                if (path == null || path.Count <= 0 || IAmInDangerZone) return;
                Vector3 myPos = _entityCache.Me.PositionWT;

                // Don't cancel during pull
                if (Fight.InFight
                    && _entityCache.Target != null
                    && !_entityCache.Target.WowUnit.InCombat) return;

                    for (int i = 0; i < path.Count - 1; i++)
                {
                    DangerZone dangerZoneOnTheWay = dangerZones
                        .FirstOrDefault(dz =>
                            dz.Position.DistanceTo(myPos) < dz.Radius + 5
                            && WTPathFinder.PointDistanceToLine(path[i], path[i + 1], dz.Position) < dz.Radius);
                    if (dangerZoneOnTheWay != null)
                    {
                        Logger.LogOnce($"Stopping move, {dangerZoneOnTheWay.Name} is on the path. Waiting despawn.");
                        if (cancelable != null) cancelable.Cancel = true;
                        MovementManager.StopMove();
                        return;
                    }
                }
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

        private void DrawEventAOE()
        {
            if (!WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar) return;

            try
            {
                if (RepositionInfo != null)
                {
                    DangerZone currentDangerZone = RepositionInfo.CurrentDangerZone;
                    ForcedSafeZone currentforcedSafeZone = RepositionInfo.ForcedSafeZone;
                    if (currentDangerZone != null)
                    {
                        Radar3D.DrawCircle(currentDangerZone.Position, currentDangerZone.Radius, Color.Orange, true, 30);
                    }

                    if (currentforcedSafeZone != null)
                    {
                        Radar3D.DrawCircle(currentforcedSafeZone.ZoneCenter, currentforcedSafeZone.Radius, Color.Blue, false, 30);
                    }
                }

                List<DangerZone> dangerZones = new List<DangerZone>(_dangerZones);
                foreach (DangerZone dangerZone in dangerZones)
                {
                    dangerZone.Draw();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }
}
