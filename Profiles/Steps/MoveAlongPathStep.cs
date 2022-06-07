using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveAlongPathStep : Step
    {
        private MoveAlongPathModel _moveAlongPathModel;
        private readonly IEntityCache _entityCache;

        public override string Name { get; }
        public override int Order { get; }

        public MoveAlongPathStep(MoveAlongPathModel stepModel, IEntityCache entityCache)
        {
            _moveAlongPathModel = stepModel;
            _entityCache = entityCache;
            Name = stepModel.Name;
            Order = stepModel.Order;
        }

        public override void Run()
        {
            if (_moveAlongPathModel.Path.Count <= 0)
            {
                Logger.LogError($"Step {Name} path is empty, skipping.");
                IsCompleted = true;
                return;
            }

            Vector3 lastPointOfPath = _moveAlongPathModel.Path.Last();

            if (_entityCache.Me.PositionWithoutType.DistanceTo(lastPointOfPath) < 5f)
            {
                if (_moveAlongPathModel.CompleteCondition == null
                    || !_moveAlongPathModel.CompleteCondition.HasCompleteCondition
                    || EvaluateCompleteCondition(_moveAlongPathModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }

            if (!MovementManager.InMovement)
            {
                MovementManager.Go(WTPathFinder.PathFromClosestPoint(_moveAlongPathModel.Path));
            }

            IsCompleted = false;
            return;
        }

        private bool CompletionConditionMet()
        {
            return _moveAlongPathModel.CompleteCondition == null
                    || !_moveAlongPathModel.CompleteCondition.HasCompleteCondition
                    || EvaluateCompleteCondition(_moveAlongPathModel.CompleteCondition);
        }

        public List<Vector3> GetMoveAlongPath => _moveAlongPathModel.Path;
    }
}
