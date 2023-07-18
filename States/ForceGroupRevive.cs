﻿using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class ForceGroupRevive : State, IState
    {
        public override string DisplayName => "Group Revive";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        private Dictionary<WoWClass, string> _rezzClasses = new Dictionary<WoWClass, string>
        {
            { WoWClass.Druid, "Revive" },
            { WoWClass.Paladin, "Redemption" },
            { WoWClass.Priest, "Resurrection" },
            { WoWClass.Shaman, "Ancestral Spirit" }
        };

        private Dictionary<string, Timer> _playerRezTimer = new Dictionary<string, Timer>(); // player name -> associated Timer

        public ForceGroupRevive(
            ICache iCache,
            IEntityCache entityCache)
        {
            _cache = iCache;
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.IsValid
                    || Fight.InFight
                    || _entityCache.EnemiesAttackingGroup.Length > 0
                    || !_cache.IsInInstance
                    || !_rezzClasses.ContainsKey(_entityCache.Me.WoWClass)
                    || !SpellManager.KnowSpell(_rezzClasses[_entityCache.Me.WoWClass])
                    || !SpellManager.SpellUsableLUA(_rezzClasses[_entityCache.Me.WoWClass])
                    || !_entityCache.ListGroupMember.Any(unit => unit.IsDead))
                {
                    return false;
                }

                // Clean up cache timers                
                foreach (var entry in _playerRezTimer.Where(kv => kv.Value.IsReady).ToList())
                {
                    _playerRezTimer.Remove(entry.Key);
                }
                
                if (!_entityCache.ListGroupMember.Any(unit => unit.IsDead && !_playerRezTimer.ContainsKey(unit.Name)))
                {
                    // Everyone is on a timer
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {

            string spell = _rezzClasses[_entityCache.Me.WoWClass];
            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            IWoWPlayer playerToResurrect = _entityCache.ListGroupMember
                .Where(unit => unit.IsDead && !_playerRezTimer.ContainsKey(unit.Name))
                .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                .FirstOrDefault();

            if (playerToResurrect != null)
            {
                Vector3 playerPos = playerToResurrect.PositionWithoutType;

                if (playerPos.DistanceTo(myPos) > 25
                    || TraceLine.TraceLineGo(myPos, playerPos, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS))
                {
                    if (!MovementManager.InMovement)
                    {
                        Logger.Log($"Going towards {playerToResurrect.Name} for resurrection");
                        List<Vector3> path = PathFinder.FindPath(myPos, playerPos);
                        MovementManager.Go(path);
                    }
                    return;
                }

                MovementManager.StopMove();

                Logger.Log($"Resurrecting {playerToResurrect.Name}");
                Interact.InteractGameObject(playerToResurrect.GetBaseAddress);
                SpellManager.CastSpellByNameLUA(spell);
                Thread.Sleep(200);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsCast)
                {
                    WoWPlayer player = ObjectManager.GetObjectWoWPlayer()
                        .FirstOrDefault(o => o.Name == playerToResurrect.Name);
                    Thread.Sleep(100);
                    if (player == null || !player.IsDead)
                    {
                        Lua.LuaDoString("SpellStopCasting();");
                    }
                }
                Interact.ClearTarget();
                _playerRezTimer.Add(playerToResurrect.Name, new Timer(10000));
            }
        }
    }
}