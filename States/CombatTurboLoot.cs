using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class CombatTurboLoot : State, IState
    {
        public override string DisplayName => "Combat Turbo Looting";
        private readonly int _lootRange = 8;

        private readonly IEntityCache _entitycache;
        private IWoWUnit _unitToLoot;
        private List<ulong> _unitsLooted = new List<ulong>();

        public CombatTurboLoot(IEntityCache entityCache)
        {
            _entitycache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !wManagerSetting.CurrentSetting.LootMobs
                    || !_entitycache.Me.Valid)
                {
                    return false;
                }

                _unitToLoot = null;
                Vector3 myPosition = _entitycache.Me.PositionWithoutType;
                List<IWoWUnit> lootableCorpses = _entitycache.LootableUnits
                    .Where(corpse => corpse.PositionWithoutType.DistanceTo(myPosition) <= _lootRange)
                    .OrderBy(corpse => corpse.PositionWithoutType.DistanceTo(myPosition))
                    .ToList();
                foreach (IWoWUnit lootableCorpse in lootableCorpses)
                {
                    if (!_unitsLooted.Contains(lootableCorpse.Guid))
                    {
                        _unitToLoot = lootableCorpse;
                        break;
                    }
                }

                return _unitToLoot != null;
            }
        }


        public override void Run()
        {
            MovementManager.StopMove();

            if (GoToTask.ToPosition(_unitToLoot.PositionWithoutType, 3f))
            {
                Interact.InteractGameObject(_unitToLoot.GetBaseAddress);
                Thread.Sleep(100);
            }

            _unitsLooted.Add(_unitToLoot.Guid);

            if (_unitsLooted.Count > 100)
            {
                _unitsLooted.RemoveRange(0, 10);
            }
        }
    }
}
