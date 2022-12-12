using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class GroupRevive : State, IState
    {
        public override string DisplayName => "Group Revive";

        private readonly ICache _cache;
        private readonly IEntityCache _entitycache;

        private Dictionary<WoWClass, string> _rezzClasses = new Dictionary<WoWClass, string>
        {
            { WoWClass.Druid, "Revive" },
            { WoWClass.Paladin, "Redemption" },
            { WoWClass.Priest, "Resurrection" },
            { WoWClass.Shaman, "Ancestral Spirit" }
        };

        private Dictionary<string, Timer> _cacheTimer = new Dictionary<string, Timer>(); // player name -> associated Timer

        public GroupRevive(ICache iCache, IEntityCache entityCache)
        {
            _cache = iCache;
            _entitycache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entitycache.Me.Valid
                    || Fight.InFight
                    || _entitycache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || !_rezzClasses.ContainsKey(_entitycache.Me.WoWClass)
                    || !SpellManager.KnowSpell(_rezzClasses[_entitycache.Me.WoWClass])
                    || !SpellManager.SpellUsableLUA(_rezzClasses[_entitycache.Me.WoWClass])
                    || !_entitycache.ListGroupMember.Any(unit => unit.Dead))
                {
                    return false;
                }

                // Clean up cache timers
                foreach (var entry in _cacheTimer.Where(kv => kv.Value.IsReady).ToList())
                {
                    _cacheTimer.Remove(entry.Key);
                }

                if (!_entitycache.ListGroupMember.Any(unit => unit.Dead && !_cacheTimer.ContainsKey(unit.Name)))
                {
                    // Everyone is on a timer
                    return false;
                }

                return true;
            }
        }

        public override void Run()
        {

            string spell = _rezzClasses[_entitycache.Me.WoWClass];
            Vector3 myPos = _entitycache.Me.PositionWithoutType;
            IWoWPlayer playerToResurrect = _entitycache.ListGroupMember
                .Where(unit => unit.Dead && !_cacheTimer.ContainsKey(unit.Name))
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
                Thread.Sleep(200);
                SpellManager.CastSpellByNameLUA(spell);
                Usefuls.WaitIsCasting();
                _cacheTimer.Add(playerToResurrect.Name, new Timer(10000));
            }
        }
    }
}
