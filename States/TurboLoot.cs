using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class TurboLoot : State, IState
    {
        public override string DisplayName => "Turbo Looting";
        private readonly int _lootRange = 20;

        private readonly IEntityCache _entitycache;
        private IWoWUnit _unitToLoot;
        private List<LootedUnit> _lootedUnits = new List<LootedUnit>();

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
                    || !_entitycache.Me.IsValid
                    || Fight.InFight)
                {
                    return false;
                }

                // Purge cache
                if (_lootedUnits.Count > 100)
                {
                    _lootedUnits.RemoveRange(0, 10);
                }
                _lootedUnits.RemoveAll(lu => lu.Timer.IsReady);

                _unitToLoot = null;
                Vector3 myPosition = _entitycache.Me.PositionWithoutType;
                List<IWoWUnit> lootableCorpses = _entitycache.LootableUnits
                    .Where(corpse => corpse?.PositionWithoutType.DistanceTo(myPosition) <= _lootRange)
                    .OrderBy(corpse => corpse?.PositionWithoutType.DistanceTo(myPosition))
                    .ToList();
                foreach (IWoWUnit lootableCorpse in lootableCorpses)
                {
                    if (!_lootedUnits.Exists(lu => lu.Guid == lootableCorpse.Guid))
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
            Vector3 myPos = _entitycache.Me.PositionWithoutType;
            Vector3 corpsePos = _unitToLoot.PositionWithoutType;

            // Loot
            if (myPos.DistanceTo(corpsePos) <= 3.5)
            {
                Logger.Log($"[TurboLoot] Looting {_unitToLoot.Name}");
                MovementManager.StopMove();
                Interact.InteractGameObject(_unitToLoot.GetBaseAddress);
                Thread.Sleep(100);
                _lootedUnits.Add(new LootedUnit(_unitToLoot.Guid));
                return;
            }

            // Approach corpse
            if (!MovementManager.InMovement ||
                MovementManager.CurrentPath.Count > 0 && MovementManager.CurrentPath.Last() != corpsePos)
            {
                MovementManager.StopMove();
                List<Vector3> pathToCorpse = PathFinder.FindPath(myPos, corpsePos, out bool resultSuccess);
                if (resultSuccess)
                {
                    MovementManager.Go(pathToCorpse);
                }
                else
                {
                    Logger.LogError($"[TurboLoot] {_unitToLoot.Name}'s corpse seems unreachable. Skipping loot.");
                    _lootedUnits.Add(new LootedUnit(_unitToLoot.Guid));
                }
            }
        }
    }
    struct LootedUnit
    {
        public ulong Guid { get; }
        public robotManager.Helpful.Timer Timer { get; }

        public LootedUnit(ulong guid)
        {
            Guid = guid;
            Timer = new robotManager.Helpful.Timer(180 * 1000);
        }
    }
}
