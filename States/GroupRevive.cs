using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager.Wow.Bot.Tasks;

namespace WholesomeDungeonCrawler.States
{
    class GroupRevive : State, IState
    {
        public override string DisplayName => "Group Revive";

        private readonly ICache _cache;
        private readonly IEntityCache _entitycache;

        private Dictionary<WoWClass, string> _rezzClasses = new Dictionary<WoWClass, string> 
        { { WoWClass.Druid, "Revive" }, { WoWClass.Paladin, "Redemption" }, { WoWClass.Priest, "Resurrection" }, { WoWClass.Shaman, "Ancestral Spirit" } };

        public GroupRevive(ICache iCache, IEntityCache entityCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
            _entitycache = entityCache;
        }
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || !_cache.IsInInstance)
                {
                    return false;
                }

                return _entitycache.ListGroupMember.Any(u => u.Dead) && _rezzClasses.ContainsKey(_entitycache.Me.WoWClass);
            }
        }
        public override void Run()
        {
            var rezzUnit = _entitycache.ListGroupMember.FirstOrDefault();
            var spell = _rezzClasses[_entitycache.Me.WoWClass];

            if(_entitycache.Me.PositionWithoutType.DistanceTo(rezzUnit.PositionWithoutType) > 25)
            {
                GoToTask.ToPosition(rezzUnit.PositionWithoutType, conditionExit: _ => _entitycache.Me.PositionWithoutType.DistanceTo(rezzUnit.PositionWithoutType) <= 25);     
            }

            Interact.InteractGameObject(rezzUnit.GetBaseAdress);
            if (SpellManager.KnowSpell(spell) && SpellManager.SpellUsableLUA(spell))
            {
                SpellManager.CastSpellByNameLUA(spell);
                Usefuls.WaitIsCasting();
            }
        }
    }
}
