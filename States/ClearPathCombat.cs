using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class ClearPathCombat : State
    {
        public override string DisplayName => "ClearPath";

        private readonly IEntityCache _entityCache;
        private IWoWUnit _unitToClear = null;
        private List<(Vector3 a, Vector3 b)> _linesToCheck = new List<(Vector3 a, Vector3 b)>();
        private List<IWoWUnit> _unitsAlongLine = new List<IWoWUnit>();

        public ClearPathCombat(IEntityCache EntityCache)
        {
            _entityCache = EntityCache;
        }

        public void Initialize()
        {
            Radar3D.OnDrawEvent += Radar3DOnDrawEvent;
            Radar3D.Pulse();
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= Radar3DOnDrawEvent;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || !_entityCache.IAmTank
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0)
                {
                    return false;
                }

                _unitsAlongLine.Clear();
                _linesToCheck.Clear();
                _unitToClear = null;
                Vector3 myPosition = _entityCache.Me.PositionWithoutType;
                _linesToCheck = MoveHelper.GetLinesToCheckOnCurrentPath(myPosition);
                _unitsAlongLine = MoveHelper.GetEnemiesAlongLines(_linesToCheck, _entityCache.EnemyUnitsList, true);

                if (_unitsAlongLine.Count > 0)
                {
                    _unitToClear = _unitsAlongLine
                        .OrderBy(unit => myPosition.DistanceTo(unit.PositionWithoutType))
                        .First();
                }

                return _unitToClear != null;
            }
        }

        public override void Run()
        {
            DisplayName = $"Clearing Path {_unitToClear.Name}";
            Logger.Log($"Clearing Path {_unitToClear.Name}");
            MovementManager.StopMove();
            Fight.StartFight(_unitToClear.Guid);
        }

        private void Radar3DOnDrawEvent()
        {
            if (Fight.InFight)
                return;

            if (_unitToClear != null)
            {
                Radar3D.DrawLine(_entityCache.Me.PositionWithoutType, _unitToClear.PositionWithoutType, Color.Red, 150);
                Radar3D.DrawCircle(_unitToClear.PositionWithoutType, 1.3f, Color.Red, true, 150);
            }

            if (_linesToCheck.Count > 0)
            {
                foreach ((Vector3 a, Vector3 b) line in _linesToCheck)
                {
                    Radar3D.DrawLine(line.a, line.b, Color.Red, 150);
                }
            }

            if (_unitsAlongLine.Count > 0)
            {
                foreach (IWoWUnit unit in _unitsAlongLine)
                {
                    Radar3D.DrawCircle(unit.PositionWithoutType, 1.3f, Color.Gray, true, 150);
                }
            }
        }
    }
}
