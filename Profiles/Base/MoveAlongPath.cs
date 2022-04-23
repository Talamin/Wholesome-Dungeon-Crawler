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
        private readonly List<Vector3> _path;
        private readonly float _randomization;
        private readonly Vector3 _target;

        public MoveAlongPath(List<Vector3> path, string stepName = "MoveAlongPath", float randomization = 0) :
            base(stepName)
        {
            _path = path;
            _randomization = randomization;
            _target = path.LastOrDefault();
        }

        public override bool Pulse()
        {
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(_target) < 5f)
            {
                IsCompleted = true;
                return true;
            }

            MovementManager.Go(WTPathFinder.PathFromClosestPoint(_path));
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
