﻿using robotManager.FiniteStateMachine;
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
        private readonly float _lootRange = 3.5f;

        private readonly IEntityCache _entityCache;
        private ICachedWoWUnit _unitToLoot;
        private List<ulong> _unitsLooted = new List<ulong>();

        public CombatTurboLoot(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!wManagerSetting.CurrentSetting.LootMobs
                    || _entityCache.EnemiesAttackingGroup.Length <= 0)
                {
                    return false;
                }

                _unitToLoot = null;
                Vector3 myPosition = _entityCache.Me.PositionWT;
                List<ICachedWoWUnit> lootables = new List<ICachedWoWUnit>(_entityCache.LootableUnits);
                List<ICachedWoWUnit> lootableCorpses = lootables
                    .Where(corpse => corpse?.PositionWT.DistanceTo(myPosition) <= _lootRange)
                    .OrderBy(corpse => corpse?.PositionWT.DistanceTo(myPosition))
                    .ToList();
                foreach (ICachedWoWUnit lootableCorpse in lootableCorpses)
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
            Vector3 myPos = _entityCache.Me.PositionWT;
            Vector3 corpsePos = _unitToLoot.PositionWT;

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
            }
        }
    }
}
