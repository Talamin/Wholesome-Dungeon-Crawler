using robotManager.Helpful;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class FollowUnitStep : Step
    {
        private FollowUnitModel _followUnitModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }

        public FollowUnitStep(FollowUnitModel followUnitModel, IEntityCache entityCache)
        {
            _followUnitModel = followUnitModel;
            _entityCache = entityCache;
            Name = followUnitModel.Name;
        }

        public override void Run()
        {
            {
                WoWUnit foundUnit = ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _followUnitModel.UnitId);
                Vector3 myPosition = _entityCache.Me.PositionWithoutType;

                if (foundUnit == null)
                {
                    if (myPosition.DistanceTo(_followUnitModel.ExpectedStartPosition) >= 15)
                    {
                        GoToTask.ToPosition(_followUnitModel.ExpectedStartPosition, 3.5f, false, context => IsCompleted);
                    }
                    else
                    {
                        if (_followUnitModel.SkipIfNotFound && EvaluateCompleteCondition(_followUnitModel.CompleteCondition))
                        {
                            Logger.LogDebug($"[Step {_followUnitModel.Name}]: Skipping. Unit {_followUnitModel.UnitId} is not here or condition is complete.");
                            IsCompleted = true;
                            return;
                        }
                        else
                        {
                            Thread.Sleep(1000);
                            Logger.LogDebug($"[Step {_followUnitModel.Name}]: Unit {_followUnitModel.UnitId} is not around and SkipIfNotFound is false. Waiting.");
                            return;
                        }
                    }
                }
                else
                {
                    if (foundUnit.Position.DistanceTo(_followUnitModel.ExpectedEndPosition) < 15
                        && EvaluateCompleteCondition(_followUnitModel.CompleteCondition))
                    {
                        Logger.LogDebug($"[Step {_followUnitModel.Name}]: {foundUnit} has reached their destination");
                        IsCompleted = true;
                        return;
                    }

                    Vector3 targetPosition = foundUnit.PositionWithoutType;
                    float followDistance = 20;

                    foreach (IWoWUnit unit in _entityCache.EnemyUnitsList)
                    {
                        if (unit.TargetGuid == foundUnit.Guid)
                        {
                            Logger.Log($"Defending Follow Unit against {unit.Name}");
                            ObjectManager.Me.Target = unit.Guid;
                            Fight.StartFight(unit.Guid, false);
                        }
                    }

                    if (!MovementManager.InMovement &&
                        _entityCache.Me.PositionWithoutType.DistanceTo(targetPosition) > followDistance)
                    {
                        GoToTask.ToPosition(targetPosition);
                    }
                }
            }
        }
    }
}
