using robotManager.Helpful;
using System.Collections.Generic;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Helpers
{
    internal class MoveHelper : IMoveHelper
    {
        public Vector3 CurrentTarget { get; set; } = Empty;

        public bool IsMovementThreadRunning { get; set; }/* || MovementManager.CurrentPath.Count > 0*/

        private static readonly Vector3 Empty = Vector3.Empty;
        private static readonly object Lock = new object();
        private static bool Running = false;

        public MoveHelper()
        {
        }

        public void MovementThreadRunning()
        {
            if (CurrentTarget != Empty)
            {
                IsMovementThreadRunning = true;
            }
            else
            {
                IsMovementThreadRunning = false;
            }
        }

        private void Wait()
        {
            if (CurrentTarget != Empty)
            {
                Running = false;
            }
        }

        public void StopAllMove(bool stopWalking = false)
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

        public void StartGoToThread(Vector3 target, string log = null)
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

        public static List<(Vector3 a, Vector3 b)> GetLinesToCheckOnCurrentPath(Vector3 myPos)
        {
            List<Vector3> currentPath = MovementManager.CurrentPath;
            List<(Vector3 a, Vector3 b)> result = new List<(Vector3, Vector3)>();
            Vector3 nextNode = MovementManager.CurrentMoveTo;
            bool nextNodeFound = false;
            float lineToCHeckDistance = 0;

            for (int i = 0; i < currentPath.Count; i++)
            {
                // break on last node unless it's the only node
                if (i >= currentPath.Count - 1 && result.Count > 0)
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
                if (result.Count > 2 && lineToCHeckDistance > 50)
                {
                    break;
                }

                // Path ahead of me
                if (result.Count <= 0)
                {
                    result.Add((myPos, currentPath[i]));
                    lineToCHeckDistance += myPos.DistanceTo(currentPath[i]);
                    if (currentPath.Count > i + 1)
                    {
                        result.Add((currentPath[i], currentPath[i + 1]));
                        lineToCHeckDistance += currentPath[i].DistanceTo(currentPath[i + 1]);
                    }
                }
                else
                {
                    result.Add((currentPath[i], currentPath[i + 1]));
                    lineToCHeckDistance += currentPath[i].DistanceTo(currentPath[i + 1]);
                }
            }

            return result;
        }

        public static List<IWoWUnit> GetEnemiesAlongLines(List<(Vector3 a, Vector3 b)> lines, IWoWUnit[] hostileUnits, bool withLos)
        {
            List<IWoWUnit> result = new List<IWoWUnit>();

            foreach ((Vector3 a, Vector3 b) line in lines)
            {
                foreach (IWoWUnit unit in hostileUnits)
                {
                    if (WTLocation.GetZDifferential(unit.PositionWithoutType) > 5
                        || WTPathFinder.PointDistanceToLine(line.a, line.b, unit.PositionWithoutType) > 20
                        || withLos && !TargetingHelper.IHaveLineOfSightOn(unit.WowUnit))
                    {
                        continue;
                    }

                    result.Add(unit);
                }
            }

            return result;
        }
    }
}
