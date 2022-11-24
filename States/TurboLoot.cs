using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache;
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
                List<IWoWUnit> _unitsToCheck = _entitycache.LootableUnits.ToList();
                foreach (IWoWUnit corpse in _unitsToCheck)
                {
                    if (!_unitsLooted.Contains(corpse.Guid)
                        && corpse.PositionWithoutType.DistanceTo(_entitycache.Me.PositionWithoutType) < _lootRange)
                    {
                        _unitToLoot = corpse;
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
                Thread.Sleep(200);

            }
            _unitsLooted.Add(_unitToLoot.Guid);

            if (_unitsLooted.Count > 100)
            {
                _unitsLooted.RemoveRange(0, 10);
            }
        }
    }
}
