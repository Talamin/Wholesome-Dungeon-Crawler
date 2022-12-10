using robotManager.Helpful;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class MoveToUnitStep : Step
    {
        private MoveToUnitModel _moveToUnitModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }
        public override int Order { get; }

        public MoveToUnitStep(MoveToUnitModel moveToUnitModel, IEntityCache entityCache)
        {
            _moveToUnitModel = moveToUnitModel;
            _entityCache = entityCache;
            Name = moveToUnitModel.Name;
            Order = moveToUnitModel.Order;
        }

        public override void Run()
        {
            WoWUnit foundUnit = ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _moveToUnitModel.UnitId);
            Vector3 myPosition = _entityCache.Me.PositionWithoutType;

            if (foundUnit == null)
            {
                if (myPosition.DistanceTo(_moveToUnitModel.ExpectedPosition) > 10)
                {
                    // Goto expected position
                    GoToTask.ToPosition(_moveToUnitModel.ExpectedPosition);
                }
                else
                {
                    if (_moveToUnitModel.SkipIfNotFound && EvaluateCompleteCondition(_moveToUnitModel.CompleteCondition))
                    {
                        Logger.LogDebug($"[Step {_moveToUnitModel.Name}]: Skipping unit {_moveToUnitModel.UnitId} because he's not here.");
                        IsCompleted = true;
                        return;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Logger.LogDebug($"[Step {_moveToUnitModel.Name}]: Unit {_moveToUnitModel.UnitId} is not around and SkipIfNotFound is false. Waiting.");
                        return;
                    }
                }
            }
            else
            {
                Vector3 targetPosition = foundUnit.PositionWithoutType;
                float targetInteractDistance = foundUnit.InteractDistance;
                GoToTask.ToPositionAndIntecractWithNpc(targetPosition, _moveToUnitModel.UnitId, _moveToUnitModel.GossipIndex);
                if (myPosition.DistanceTo(targetPosition) < targetInteractDistance
                    && EvaluateCompleteCondition(_moveToUnitModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }
        }
    }
}
