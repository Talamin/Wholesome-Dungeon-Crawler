using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class MovementSlaveBETA : State
    {
        public override string DisplayName => "Slave";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public MovementSlaveBETA(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }


        //BUILDING STRICT FOLLOW PATH
        private Vector3 strictoldleaderpos = new Vector3(0, 0, 0);
        private List<Vector3> strictfollowpath = new List<Vector3>();
        private List<Vector3> strictfollowpathnew = new List<Vector3>();
        //
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || Fight.InFight)
                {
                    return false;
                }

                if (_entityCache.IAmTank)
                {
                    return false;
                }

                IWoWUnit Tank = _entityCache.TankUnit;
                if (Tank == null)
                {
                    Logger.Log("No IWoWUnit Tankunit");
                    return false;
                }


                //Checks when not to follow
                //If we are moving, return false
                if (MovementManager.InMoveTo || MovementManager.InMovement)
                {
                    return false;
                }

                //BUILDING A STRICT FOLLOWPATH
                if (strictoldleaderpos == new Vector3(0, 0, 0))
                {
                    strictoldleaderpos = Tank.PositionWithoutType;
                    Logger.Log("Set strictoldleaderpos: " + strictoldleaderpos);
                    strictfollowpath.Add(strictoldleaderpos);
                    Logger.Log("Added strictoldleaderpos to strictfollowpath");
                }
                if (Tank.PositionWithoutType.DistanceTo(strictoldleaderpos) > 2)
                {
                    strictoldleaderpos = Tank.PositionWithoutType;
                    strictfollowpath.Add(Tank.PositionWithoutType);
                    Logger.Log("Added new Vector: " + Tank.PositionWithoutType);
                }
                if (_entityCache.Me.PositionWithoutType.DistanceTo(strictfollowpath[0]) <= 2 && strictfollowpath.Count() > 1)
                {
                    strictfollowpath.Remove(strictfollowpath[0]);
                    Logger.Log("Removed old Vector: " + strictfollowpath[0]);
                }

                //If we are in Range of our Tank
                if (_entityCache.Me.PositionWithoutType.DistanceTo(strictoldleaderpos) <= 15)
                {
                    return false;
                }

                if (_entityCache.Me.PositionWithoutType.DistanceTo(strictoldleaderpos) > 15
                    || TraceLine.TraceLineGo(Tank.PositionWithoutType))
                {
                    return true;
                }
                return false;
            }
        }

        public override void Run()
        {
            Vector3 point = strictfollowpath[0];
            Logger.Log("Moveto: " + point);
            MovementManager.MoveTo(point);
            //GoToTask.ToPosition(point, 1f, false, context => _entityCache.Me.PositionWithoutType.DistanceTo2D(point) > 2);
        }
    }
}
