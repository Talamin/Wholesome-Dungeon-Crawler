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
    class CombatTurboLoot : State, IState
    {
        public override string DisplayName => "Combat Turbo Looting";
        private readonly int _lootRange = 8;

        private readonly IEntityCache _entityCache;
        private IWoWUnit _unitToLoot;
        private List<ulong> _unitsLooted = new List<ulong>();

        public CombatTurboLoot(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !wManagerSetting.CurrentSetting.LootMobs
                    || !_entityCache.Me.Valid
                    || _entityCache.EnemiesAttackingGroup.Length <= 0)
                {
                    return false;
                }

                _unitToLoot = null;
                Vector3 myPosition = _entityCache.Me.PositionWithoutType;
                List<IWoWUnit> lootableCorpses = _entityCache.LootableUnits
                    .Where(corpse => corpse?.PositionWithoutType.DistanceTo(myPosition) <= _lootRange)
                    .OrderBy(corpse => corpse?.PositionWithoutType.DistanceTo(myPosition))
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
            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            Vector3 corpsePos = _unitToLoot.PositionWithoutType;

            // Purge cache
            if (_unitsLooted.Count > 100)
            {
                _unitsLooted.RemoveRange(0, 10);
            }

            // Loot
            if (myPos.DistanceTo(corpsePos) <= 3.5)
            {
                Logger.Log($"[CombatLoot] Looting {_unitToLoot.Name}");
                MovementManager.StopMove();
                Interact.InteractGameObject(_unitToLoot.GetBaseAddress);
                Thread.Sleep(100);
                _unitsLooted.Add(_unitToLoot.Guid);
                return;
            }

            // Approach corpse
            if (!MovementManager.InMovement
                || MovementManager.CurrentPath.Count > 0 && MovementManager.CurrentPath.Last() != corpsePos)
            {
                MovementManager.StopMove();
                List<Vector3> pathToCorpse = PathFinder.FindPath(myPos, corpsePos, out bool resultSuccess);
                if (resultSuccess)
                {
                    MovementManager.Go(pathToCorpse);
                }
                else
                {
                    Logger.LogError($"[CombatLoot] {_unitToLoot.Name}'s corpse seems unreachable. Skipping loot.");
                    _unitsLooted.Add(_unitToLoot.Guid);
                }
            }
        }
    }
}
