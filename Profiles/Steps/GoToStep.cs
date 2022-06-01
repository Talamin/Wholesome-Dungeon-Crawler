using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using wManager.Wow.Bot.Tasks;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class GoToStep : Step
    {
        private GoToModel _gotoModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }

        public GoToStep(GoToModel goToModel, IEntityCache entityCache)
        {
            _gotoModel = goToModel;
            _entityCache = entityCache;
            Name = goToModel.Name;
        }

        public override void Run()
        {
            float precision = _gotoModel.Precision <= 0 ? 5 : _gotoModel.Precision;
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_gotoModel.TargetPosition) < precision)
            {
                if (!_gotoModel.CompleteCondition.HasCompleteCondition)
                {
                    IsCompleted = true;
                    return;
                }
                else if (EvaluateCompleteCondition(_gotoModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }

            GoToTask.ToPosition(_gotoModel.TargetPosition);
            IsCompleted = false;
        }
    }
}