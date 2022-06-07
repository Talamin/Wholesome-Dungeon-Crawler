using robotManager.Helpful;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class DefendSpotStep : Step
    {
        private DefendSpotModel _defendSpotModel;
        private readonly IEntityCache _entityCache;

        public override string Name { get; }

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
            Name = _defendSpotModel.Name;
        }

        private Timer stepTimer = new Timer();

        public override void Run()
        {
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) < _defendSpotModel.Precision && stepTimer.IsReady)
            {
                if (!_defendSpotModel.CompleteCondition.HasCompleteCondition)
                {
                    IsCompleted = true;
                    return;
                }
                else if (EvaluateCompleteCondition(_defendSpotModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }

            if (!MovementManager.InMovement || MovementManager.CurrentMoveTo.DistanceTo(_defendSpotModel.DefendPosition) > _defendSpotModel.Precision)
            {
                GoToTask.ToPosition(_defendSpotModel.DefendPosition);
            }

            IsCompleted = false;

        }
    }
}
