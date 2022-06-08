using robotManager.Helpful;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class DefendSpotStep : Step
    {
        private DefendSpotModel _defendSpotModel;
        private readonly IEntityCache _entityCache;

        public override string Name { get; }
        public override int Order { get; }

        private Timer stepTimer;

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
            Name = _defendSpotModel.Name;
            Order = _defendSpotModel.Order;
        }



        public override void Run()
        {
            if(stepTimer == null)
            {
                stepTimer = new Timer(new System.TimeSpan(0, 0, _defendSpotModel.Timer));
            }
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

            foreach(var unit in _entityCache.EnemyUnitsList)
            {
                if(unit.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) <= 15)
                {
                    Logger.Log("Defending my Spot");
                    ObjectManager.Me.Target = unit.Guid;
                    Fight.StartFight(unit.Guid, false);
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
