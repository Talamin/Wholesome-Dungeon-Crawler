using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class DefendSpotStep : Step
    {
        private DefendSpotModel _defendSpotModel;
        private readonly IEntityCache _entityCache;
        private Timer _stepTimer;
        private int _defendSpotRadius;
        private int _timeToWaitInMilliseconds;

        public override string Name { get; }

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache) : base(defendSpotModel.CompleteCondition)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
            _defendSpotRadius = _defendSpotModel.DefendSpotRadius;
            _defendSpotRadius = _defendSpotRadius < 5 ? 5 : _defendSpotRadius;
            _timeToWaitInMilliseconds = _defendSpotModel.Timer * 1000;
            Name = _defendSpotModel.Name;
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (_stepTimer == null)
            {
                _stepTimer = new Timer(_timeToWaitInMilliseconds);
            }

            if (!MovementManager.InMovement 
                && _entityCache.Me.PositionWT.DistanceTo(_defendSpotModel.DefendPosition) > _defendSpotRadius)
            {
                List<Vector3> pathToCenter = PathFinder.FindPath(_entityCache.Me.PositionWT, _defendSpotModel.DefendPosition);
                MovementManager.Go(pathToCenter);
            }

            IWoWUnit unitToAttack = _entityCache.EnemyUnitsList
                .Where(unit => unit.PositionWT.DistanceTo(_defendSpotModel.DefendPosition) <= _defendSpotRadius
                    && unit.Reaction <= wManager.Wow.Enums.Reaction.Hostile)
                .OrderBy(unit => unit.PositionWT.DistanceTo(_defendSpotModel.DefendPosition))
                .FirstOrDefault();

            if (unitToAttack != null)
            {
                Logger.Log($"Defending spot against {unitToAttack.Name}");
                ObjectManager.Me.Target = unitToAttack.Guid;
                Fight.StartFight(unitToAttack.Guid, false);
                return;
            }

            if (_entityCache.Me.PositionWT.DistanceTo(_defendSpotModel.DefendPosition) <= _defendSpotRadius
                && _stepTimer.IsReady
                && EvaluateCompleteCondition())
            {
                IsCompleted = true;
                return;
            }
        }

        public double GetTimeLeft => _stepTimer == null || _stepTimer.IsReady ? 0 : _stepTimer.TimeLeft();
    }
}
