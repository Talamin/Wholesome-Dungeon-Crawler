using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

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
                    || !_cache.IsInInstance)
                {
                    return false;
                }
                if(_rezzClasses.ContainsKey(_entitycache.Me.WoWClass) && !SpellManager.KnowSpell(_rezzClasses[_entitycache.Me.WoWClass]))
                {
                    return false;
                }

                return _entitycache.ListGroupMember.Any(u => u.Dead) && _rezzClasses.ContainsKey(_entitycache.Me.WoWClass);
            }
        }
        public override void Run()
        {
            string spell = _rezzClasses[_entitycache.Me.WoWClass];
            List<IWoWPlayer> playersToResurrect = _entitycache.ListGroupMember
                .Where(u => u.Dead)
                .ToList();

            foreach (IWoWPlayer player in playersToResurrect)
            {
                if (_entitycache.Me.PositionWithoutType.DistanceTo(player.PositionWithoutType) > 25)
                {
                    GoToTask.ToPosition(player.PositionWithoutType, conditionExit: _ => _entitycache.Me.PositionWithoutType.DistanceTo(player.PositionWithoutType) <= 25 && !TraceLine.TraceLineGo(player.PositionWithoutType));
                }

                Interact.InteractGameObject(player.GetBaseAddress);
                if (SpellManager.KnowSpell(spell) && SpellManager.SpellUsableLUA(spell))
                {
                    SpellManager.CastSpellByNameLUA(spell);
                    Usefuls.WaitIsCasting();
                    Thread.Sleep(2000);
                    playersToResurrect.Remove(player);
                }
            }
        }
    }
}
