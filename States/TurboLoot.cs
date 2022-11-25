using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class TurboLoot : State, IState
    {
        public override string DisplayName => "Turbo Looting";
        private readonly int _lootRange = 20;

        private readonly IEntityCache _entitycache;
        private IWoWUnit _unitToLoot;
        private List<ulong> _unitsLooted = new List<ulong>();

        public TurboLoot(IEntityCache entityCache)
        {
            _entitycache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !wManagerSetting.CurrentSetting.LootMobs
                    || !_entitycache.Me.Valid
                    || Fight.InFight
                    || Bag.GetContainerNumFreeSlots <= 2)
                {
                    return false;
                }

                _unitToLoot = null;
                Vector3 myPosition = _entitycache.Me.PositionWithoutType;
                List<IWoWUnit> lootableCorpses = _entitycache.LootableUnits
                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(myPosition))
                    .ToList();
                lootableCorpses.RemoveAll(corpse => corpse.PositionWithoutType.DistanceTo(myPosition) > _lootRange);
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
