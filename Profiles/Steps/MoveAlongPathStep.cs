using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveAlongPathStep : Step
    {
        private MoveAlongPathModel _moveAlongPathModel;

        public MoveAlongPathStep(MoveAlongPathModel stepModel)
        {
            _moveAlongPathModel = stepModel;
        }

        public override void Run()
        {
            Vector3 lastPointOfPath = _moveAlongPathModel.Path.Last();
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(lastPointOfPath) < 5f)
            {
                IsCompleted = true;
                return;
            }

            MovementManager.Go(WTPathFinder.PathFromClosestPoint(_moveAlongPathModel.Path)); //maybe spamming
            /*
            if (!_movehelper.IsMovementThreadRunning || _movehelper.CurrentTarget.DistanceTo(_target) > 2)
            {
                //_movehelper.StartMoveAlongThread(PathFromClosestPoint(_path));
            }
            */
            IsCompleted = false;
            return;
        }
    }
}
