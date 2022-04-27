using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class MoveAlongPath : Step
    {
        public List<Vector3> Path { get; set; }
        public float Randomization { get; set; }
        public Vector3 Target { get; set; }

        public MoveAlongPath(List<Vector3> path, string stepName = "MoveAlongPath", float randomization = 0) :
            base(stepName)
        {
            Path = path;
            Randomization = randomization;
            Target = path.LastOrDefault();
        }

        public override bool Pulse()
        {
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(Target) < 5f)
            {
                IsCompleted = true;
                return true;
            }

            MovementManager.Go(WTPathFinder.PathFromClosestPoint(Path));
            /*
            if (!_movehelper.IsMovementThreadRunning || _movehelper.CurrentTarget.DistanceTo(_target) > 2)
            {
                //_movehelper.StartMoveAlongThread(PathFromClosestPoint(_path));
            }
            */
            return IsCompleted = false;
        }

    }
}
