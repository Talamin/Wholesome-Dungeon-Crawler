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
            { WoWClass.Priest, new List<string>() { "Lesser Heal", "Heal" } },
            { WoWClass.Shaman, new List<string>() { "Healing Wave", "Lesser Healing Wave" } }
        };

        public ForceOOCHeal(
            ICache iCache,
            IEntityCache entityCache)
        {
            _cache = iCache;
            _entityCache = entityCache;
            _iAmHealer = _healClasses.ContainsKey(_entityCache.Me.WoWClass);
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
                Vector3 myPos = _entityCache.Me.PositionWithoutType;
                IWoWPlayer playerToHeal = _entityCache.ListGroupMember
                    .Where(unit => unit.IsValid && !unit.IsDead && unit.HealthPercent <= _healThreshold)
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPos))
                    .FirstOrDefault();

                if (playerToHeal != null)
                {
                    Vector3 playerPos = playerToHeal.PositionWithoutType;

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
                    Usefuls.WaitIsCasting();
                    Interact.ClearTarget();
                }
            }
            else // Others logic
            {
                Logger.LogOnce("Waiting to get healed");
                MovementManager.StopMove();
                Thread.Sleep(1000);
            }
        }
    }
}
