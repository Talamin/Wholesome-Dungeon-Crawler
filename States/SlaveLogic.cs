using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class SlaveLogic : State
    {
        //The Idea: Having a State which checks permanently if the Tank is infront of us and near our path.
        //If he´s not, the State will be true, and we perform some action.
        //If he is, the State will be false, and we perform the Profile  Logic.
        public override string DisplayName => "Slave";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private string tankname = "Tank";

        public SlaveLogic(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        private IWoWUnit _tankUnit = null;
        public List<(Vector3 a, Vector3 b)> LinesToCheck = new List<(Vector3 a, Vector3 b)>(); // For Radar 3D

        public override bool NeedToRun
        { 
        get
            {
                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || !_entityCache.Me.InCombatFlagOnly
                    || Fight.InFight
                    || MovementManager.CurrentPath == null
                    || MovementManager.CurrentPath.Count <= 0
                    || (!MovementManager.InMoveTo && !MovementManager.InMovement)
                    || MovementManager.CurrentPath.Last().DistanceTo(_entityCache.Me.PositionWithoutType) > 120)
                {
                    return false;
                }

                if(_entityCache.Me.Name == tankname)
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
                //Now we check if the Tank is along the lines ahead of us
                IWoWUnit Tankunit = _entityCache.ListGroupMember.Where(unit => unit.Name == tankname).FirstOrDefault();
                foreach ((Vector3 a, Vector3 b) line in linesToCheck)
                {
                    if(!IHaveLineOfSightOn(Tankunit) && WTPathFinder.PointDistanceToLine(line.a, line.b, Tankunit.PositionWithoutType) < 20)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public override void Run()
        { 
            //do some logic if the Tank is not running infront of us.
        }
        private bool IHaveLineOfSightOn(IWoWUnit woWUnit)
        {
            Vector3 myPos = _entityCache.Me.PositionWithoutType;
            return !TraceLine.TraceLineGo(myPos, woWUnit.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
        }
    }
}
