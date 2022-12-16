using robotManager.Helpful;
using System.Linq;
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
        private float _precision;
        private Timer _stepTimer;
        public override string Name { get; }
        public override int Order { get; }

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
            _precision = _defendSpotModel.Precision;
            Name = _defendSpotModel.Name;
            Order = _defendSpotModel.Order;
        }

        public override void Run()
        {
            if (_precision < 5)
            {
                _precision = 5;
            }

            if (_stepTimer == null)
            {
                _stepTimer = new Timer(_defendSpotModel.Timer);
            }

            if (!MovementManager.InMovement && _entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) > _precision)
            {
                GoToTask.ToPosition(_defendSpotModel.DefendPosition);
            }

            IWoWUnit unitToAttack = _entityCache.EnemyUnitsList
                .Where(unit => unit.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) <= 30)
                .FirstOrDefault();

            if (unitToAttack != null)
            {
                Logger.Log($"Defending spot against {unitToAttack.Name}");
                ObjectManager.Me.Target = unitToAttack.Guid;
                Fight.StartFight(unitToAttack.Guid, false);
                return;
            }

            if (_entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) <= _precision
                && _stepTimer.IsReady
                && EvaluateCompleteCondition(_defendSpotModel.CompleteCondition))
            {
                IsCompleted = true;
                return;
            }
        }
    }
}
