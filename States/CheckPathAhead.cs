using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    internal class CheckPathAhead : State
    {
        public override string DisplayName { get; set; } = "Check Path Ahead";

        private readonly IEntityCache _entityCache;
        private readonly IPartyChatManager _partyChatManager;
        private IWoWUnit _unitOnPath = null;
        private Timer _broadcastTimer = new Timer();

        public CheckPathAhead(IEntityCache EntityCache, IPartyChatManager partyChatManager)
        {
            _entityCache = EntityCache;
            _partyChatManager = partyChatManager;
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
                    || _entityCache.IAmTank && MyTeamIsAround
                    /*|| !_entityCache.IAmTank && _entityCache.TankUnit?.TargetGuid != 0*/)
                {
                    return false;
                }

                _unitOnPath = null;
                List<Vector3> currentPath = MovementManager.CurrentPath;
                Vector3 nextNode = MovementManager.CurrentMoveTo;
                Vector3 myPosition = _entityCache.Me.PositionWithoutType;
                List<(Vector3 a, Vector3 b)> linesToCheck = new List<(Vector3, Vector3)>();
                bool nextNodeFound = false;
                for (int i = 0; i < currentPath.Count; i++)
                {
                    // break on last node unless it's the only node
                    if (i >= currentPath.Count - 1 && linesToCheck.Count > 0)
                    {
                        break;
                    }

                    // skip nodes behind me
                    if (!nextNodeFound)
                    {
                        if (currentPath[i] != nextNode)
                        {
                            continue;
                        }
                        nextNodeFound = true;
                    }

                    // Ignore if too far
                    if (linesToCheck.Count > 2 && currentPath[i].DistanceTo(myPosition) > 50)
                    {
                        break;
                    }

                    // Path ahead of me
                    if (linesToCheck.Count <= 0)
                    {
                        linesToCheck.Add((myPosition, currentPath[i]));
                        if (currentPath.Count > i + 1)
                        {
                            linesToCheck.Add((currentPath[i], currentPath[i + 1]));
                        }
                    }
                    else
                    {
                        linesToCheck.Add((currentPath[i], currentPath[i + 1]));
                    }
                }

                // Check if enemies along the lines
                IWoWUnit[] hostileUnits = _entityCache.EnemyUnitsList;
                // Check for hostiles along the lines
                List<IWoWUnit> unitsAlongLine = new List<IWoWUnit>();
                foreach ((Vector3 a, Vector3 b) line in linesToCheck)
                {
                    foreach (IWoWUnit unit in hostileUnits)
                    {
                        if (WTLocation.GetZDifferential(unit.PositionWithoutType) > 5
                            || WTPathFinder.PointDistanceToLine(line.a, line.b, unit.PositionWithoutType) > 20)
                        {
                            continue;
                        }
                        _unitOnPath = unit;
                        break;
                    }

                    if (_unitOnPath != null)
                    {
                        break;
                    }
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
    }
}