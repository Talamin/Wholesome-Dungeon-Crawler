using robotManager.Helpful;
using System.Threading;
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
    }
}
