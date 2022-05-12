using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class TargetingCombat : State
    {
        public override string DisplayName => "Targeting";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private string tankname = "Tank";

        public TargetingCombat(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        private IWoWUnit _unitToClear = null;
        public List<(Vector3 a, Vector3 b)> LinesToCheck = new List<(Vector3 a, Vector3 b)>(); // For Radar 3D
        public override bool NeedToRun 
        {
            get
            {
                if(!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || !_entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0
                    || (!MovementManager.InMoveTo && !MovementManager.InMovement)
                    ||  MovementManager.CurrentPath.Last().DistanceTo(_entityCache.Me.PositionWithoutType) > 120) 
                { 
                    return false;
                }

                if(_entityCache.Me.Name != tankname)
                {
                    return false;
                }

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
                LinesToCheck = linesToCheck;

                // Check if enemies along the lines
                IWoWUnit[] hostileUnits = _entityCache.HostileUnits;
                // Check for hostiles along the lines
                List<IWoWUnit> unitsAlongLine = new List<IWoWUnit>();
                foreach ((Vector3 a, Vector3 b) line in linesToCheck)
                {
                    if (_unitToClear == null)
                    {
                        foreach (IWoWUnit unit in hostileUnits)
                        {
                            if (!IHaveLineOfSightOn(unit))
                            {
                                continue;
                            }
                            if (WTLocation.GetZDifferential(unit.PositionWithoutType) < 5
                                && WTPathFinder.PointDistanceToLine(line.a, line.b, unit.PositionWithoutType) < 20)
                            {
                                unitsAlongLine.Add(unit);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (unitsAlongLine.Count > 0)
                {
                    _unitToClear = unitsAlongLine
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
            Fight.StartFight(_unitToClear.Guid);
            _unitToClear = null;

        }

        private bool IHaveLineOfSightOn(IWoWUnit woWUnit)
        {
            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            return !TraceLine.TraceLineGo(myPos, woWUnit.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS); 
        }
    }
}
