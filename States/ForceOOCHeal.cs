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
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class ForceOOCHeal : State, IState
    {
        public override string DisplayName => "Force Heal";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly int _healThreshold = 60; // Setting ?
        private readonly bool _iAmHealer;
        private string _healSPell = null;

        private Dictionary<WoWClass, List<string>> _healClasses = new Dictionary<WoWClass, List<string>>
        {
            { WoWClass.Druid, new List<string>() { "Healing Touch" } },
            { WoWClass.Paladin, new List<string>() { "Holy Light" } },
            { WoWClass.Priest, new List<string>() { "Heal", "Lesser Heal" } },
            { WoWClass.Shaman, new List<string>() { "Lesser Healing Wave", "Healing Wave" } }
        };

        public ForceOOCHeal(
            ICache iCache,
            IEntityCache entityCache)
        {
            _cache = iCache;
            _entityCache = entityCache;
            _iAmHealer = _healClasses.ContainsKey(_entityCache.Me.WoWClass)
                && CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Heal;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.IsValid
                    || Fight.InFight
                    || !_cache.IsInInstance
                    || _entityCache.EnemiesAttackingGroup.Length > 0
                    || _entityCache.ListGroupMember.All(unit => unit.HealthPercent > _healThreshold)
                    || _entityCache.ListGroupMember.Any(unit => unit.IsDead))
                {
                    return false;
                }

                // Healer Logic
                if (_iAmHealer)
                {
                    _healSPell = _healClasses[_entityCache.Me.WoWClass]
                        .FirstOrDefault(healSpell => SpellManager.KnowSpell(healSpell) && SpellManager.SpellUsableLUA(healSpell));

                    return _healSPell != null;
                }

                // Others logic
                return true;
            }
        }

        public override void Run()
        {
            // Healer Logic
            if (_iAmHealer)
            {
                Vector3 myPos = _entityCache.Me.PositionWT;
                IWoWPlayer playerToHeal = _entityCache.ListGroupMember
                    .Where(unit => unit.IsValid && !unit.IsDead && unit.HealthPercent <= _healThreshold)
                    .OrderBy(unit => unit.PositionWT.DistanceTo(myPos))
                    .FirstOrDefault();

                if (playerToHeal != null)
                {
                    Vector3 playerPos = playerToHeal.PositionWT;

                    if (playerPos.DistanceTo(myPos) > 25
                        || TraceLine.TraceLineGo(myPos, playerPos, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS))
                    {
                        if (!MovementManager.InMovement)
                        {
                            Logger.Log($"Going towards {playerToHeal.Name} for healing");
                            List<Vector3> path = PathFinder.FindPath(myPos, playerPos);
                            MovementManager.Go(path);
                        }
                        return;
                    }

                    MovementManager.StopMove();

                    Logger.Log($"Healing {playerToHeal.Name}");
                    Interact.InteractGameObject(playerToHeal.GetBaseAddress);
                    Thread.Sleep(200);
                    SpellManager.CastSpellByNameLUA(_healSPell);
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsCast)
                    {
                        Thread.Sleep(100);
                        if (playerToHeal == null || playerToHeal.HealthPercent >= _healThreshold)
                        {
                            Lua.LuaDoString("SpellStopCasting();");
                        }
                    }
                    Interact.ClearTarget();
                }
            }
            else // Others logic
            {
                Logger.LogOnce("Waiting for heals");
                MovementManager.StopMove();
                Thread.Sleep(500);
            }
        }
    }
}
