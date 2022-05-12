using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveAlongPathStep : Step
    {
        private MoveAlongPathModel _moveAlongPathModel;
        private readonly IEntityCache _entityCache;

        public MoveAlongPathStep(MoveAlongPathModel stepModel, IEntityCache entityCache)
        {
            _moveAlongPathModel = stepModel;
            _entityCache = entityCache;
        }

        public override void Run()
        {
            Vector3 lastPointOfPath = _moveAlongPathModel.Path.Last();
            if (_entityCache.Me.PositionWithoutType.DistanceTo(lastPointOfPath) < 5f)
            {
                //if (!_moveAlongPathModel.CompleteCondition.HasCompleteCondition)
                //{
                    IsCompleted = true;
                    return;
                //}
                //else if (EvaluateCompleteCondition(_moveAlongPathModel.CompleteCondition))
                //{
                //    IsCompleted = true;
                //    return;
                //}
            }
            if (!MovementManager.InMovement)
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
