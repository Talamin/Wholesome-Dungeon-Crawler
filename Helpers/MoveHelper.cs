using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Helpers
{
    public class MoveHelper
    {
        private static readonly Vector3 Empty = Vector3.Empty;
        private static readonly object Lock = new object();
        private static bool Running = false;

        private static void Wait()
        {
            if (CurrentTarget != Empty)
            {
                Running = false;
            }
        }

        public static void StopAllMove(bool stopWalking = false)
        {
            lock (Lock)
            {
                Wait();
            }
            if (stopWalking)
            {
                MovementManager.StopMove();
                MovementManager.StopMoveTo();
            }
        }

        public static Vector3 CurrentTarget { get; private set; } = Empty;

        public static bool IsMovementThreadRunning => CurrentTarget != Empty/* || MovementManager.CurrentPath.Count > 0*/;

        public static void StartGoToThread(Vector3 target, string log = null)
        {
            lock (Lock)
            {
                if (CurrentTarget.Equals(target))
                {
                    return;
                }

                Wait();

                CurrentTarget = target;
                Running = true;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    if (log != null)
                    {
                        Logger.Log(log);
                    }
                    GoToTask.ToPosition(target, 0.5f, conditionExit: _ => Running);
                    //Logger.LogDebug($"GoToTask finished towards {target}");
                    lock (Lock)
                    {
                        CurrentTarget = Empty;
                        Monitor.Pulse(Lock);
                    }
                });
            }
        }

        public static void StartMoveAlongThread(List<Vector3> path, string log = null)
        {
            lock (Lock)
            {
                if (CurrentTarget.Equals(MovementManager.CurrentMoveTo))
                {
                    return;
                }

                var pathIndex  = WTPathFinder.GetIndexOfClosestPoint(path);

                Wait();

                CurrentTarget = path[pathIndex];
                Running = true;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    if (log != null)
                    {
                        Logger.Log(log);
                    }
                    GoToTask.ToPosition(MovementManager.CurrentMoveTo, 0.5f, conditionExit: _ => Running);
                    //Logger.LogDebug($"GoToTask finished towards {target}");
                    lock (Lock)
                    {
                        CurrentTarget = Empty;
                        Monitor.Pulse(Lock);
                    }
                });
            }
        }
    }
}
