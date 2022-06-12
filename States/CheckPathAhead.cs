using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    internal class CheckPathAhead : State
    {
        public override string DisplayName { get; set; } = "Check Path Ahead";

        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private Timer _broadcastTimer = new Timer();
        private IWoWUnit _unitOnPath = null;
        private List<(Vector3 a, Vector3 b)> _linesToCheck = new List<(Vector3 a, Vector3 b)>();
        private List<IWoWUnit> _unitsAlongLine = new List<IWoWUnit>();

        public CheckPathAhead(IEntityCache EntityCache, IPartyChatManager partyChatManager)
        {
            _entityCache = EntityCache;
            _partyChatManager = partyChatManager;
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
                if (!_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0
                    || !Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || _entityCache.IAmTank && MyTeamIsAround)
                {
                    return false;
                }

                _unitsAlongLine.Clear();
                _linesToCheck.Clear();
                _unitOnPath = null;
                Vector3 myPosition = _entityCache.Me.PositionWithoutType;
                _linesToCheck = MoveHelper.GetLinesToCheckOnCurrentPath(_entityCache.Me.PositionWithoutType);
                _unitsAlongLine = MoveHelper.GetEnemiesAlongLines(_linesToCheck, _entityCache.EnemyUnitsList, false);

                if (_unitsAlongLine.Count > 0)
                {
                    _unitOnPath = _unitsAlongLine
                        .OrderBy(unit => myPosition.DistanceTo(unit.PositionWithoutType))
                        .First();
                }

                // the tank is closer from the unit, we can go
                if (_unitOnPath != null
                    && !_entityCache.IAmTank
                    && _entityCache.TankUnit != null
                    && _entityCache.TankUnit.PositionWithoutType.DistanceTo(_unitOnPath.PositionWithoutType) + 10 < _entityCache.Me.PositionWithoutType.DistanceTo(_unitOnPath.PositionWithoutType))
                {
                    return false;
                }

                return _unitOnPath != null;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();

            if (_broadcastTimer.IsReady)
            {
                if (_entityCache.IAmTank)
                {
                    Logger.Log($"{_unitOnPath.Name} is on the way. Waiting for the team to move their frail asses.");
                    _broadcastTimer = new Timer(1000 * 10);
                }
                else
                {
                    if (_entityCache.TankUnit != null)
                    {
                        Logger.Log($"{_unitOnPath.Name} is on the way. Waiting for the tank to move his fat ass.");
                        _broadcastTimer = new Timer(1000 * 10);
                    }
                    else
                    {
                        _partyChatManager.Broadcast(PartyChatManager.ChatMessageType.ASSIST_WITH_ENEMIES_AHEAD, null);
                        _broadcastTimer = new Timer(1000 * 10);
                    }
                }
            }
        }

        private bool MyTeamIsAround => _entityCache.ListGroupMember.Length == _entityCache.ListPartyMemberNames.Count
                    && _entityCache.ListGroupMember.All(member => member.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) < 40);

        private void Radar3DOnDrawEvent()
        {
            if (Fight.InFight)
                return;

            if (_unitOnPath != null)
            {
                Radar3D.DrawLine(_entityCache.Me.PositionWithoutType, _unitOnPath.PositionWithoutType, Color.PaleTurquoise, 150);
                Radar3D.DrawCircle(_unitOnPath.PositionWithoutType, 1.3f, Color.PaleTurquoise, true, 150);
            }

            if (_linesToCheck.Count > 0)
            {
                foreach ((Vector3 a, Vector3 b) line in _linesToCheck)
                {
                    Radar3D.DrawLine(line.a, line.b, Color.PaleTurquoise, 150);
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