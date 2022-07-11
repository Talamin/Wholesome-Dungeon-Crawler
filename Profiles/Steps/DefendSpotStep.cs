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

        private float Precision;

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
            Name = _defendSpotModel.Name;
            Order = _defendSpotModel.Order;
            Precision = _defendSpotModel.Precision;
        }



        public override void Run()
        {
            if(Precision == 0)
            {
                Precision = 5;
            }
            if (stepTimer == null)
            {
                stepTimer = new Timer(_defendSpotModel.Timer);
                //Logger.Log("Set stepTimer to: " + _defendSpotModel.Timer);
            }
            //Logger.Log("Distance To Point: " + _entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition));
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) <= Precision && stepTimer.IsReady)
            {
                //Logger.Log("Steptimer Ready? " + stepTimer.IsReady);
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

            foreach (var unit in _entityCache.EnemyUnitsList)
            {
                if (unit.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) <= 20)
                {
                    Logger.Log("Defending my Spot");
                    ObjectManager.Me.Target = unit.Guid;
                    Fight.StartFight(unit.Guid, false);
                }
            }

            //Logger.Log("Steptimer Ready? " + stepTimer.IsReady);

            if (!MovementManager.InMovement || _entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) > Precision)
            {
                GoToTask.ToPosition(_defendSpotModel.DefendPosition);
            }

            IsCompleted = false;

        }
    }
}
