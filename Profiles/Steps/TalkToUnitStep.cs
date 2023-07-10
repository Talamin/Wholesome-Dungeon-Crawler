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
    public class TalkToUnitStep : Step
    {
        private TalkToUnitModel _talkToUnitModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }

        public TalkToUnitStep(TalkToUnitModel talkToUnitModel, IEntityCache entityCache) : base(talkToUnitModel.CompleteCondition)
        {
            _talkToUnitModel = talkToUnitModel;
            _entityCache = entityCache;
            Name = talkToUnitModel.Name;
        }

        public override void Run()
        {
            WoWUnit foundUnit = ObjectManager.GetObjectWoWUnit().FirstOrDefault(unit => unit.Entry == _talkToUnitModel.UnitId);
            Vector3 myPosition = _entityCache.Me.PositionWithoutType;

            if (foundUnit == null)
            {
                if (myPosition.DistanceTo(_talkToUnitModel.ExpectedPosition) > 10)
                {
                    // Goto expected position
                    GoToTask.ToPosition(_talkToUnitModel.ExpectedPosition);
                }
                else
                {
                    if (_talkToUnitModel.SkipIfNotFound && EvaluateCompleteCondition())
                    {
                        Logger.LogDebug($"[Step {_talkToUnitModel.Name}]: Skipping unit {_talkToUnitModel.UnitId} because he's not here.");
                        IsCompleted = true;
                        return;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Logger.LogDebug($"[Step {_talkToUnitModel.Name}]: Unit {_talkToUnitModel.UnitId} is not around and SkipIfNotFound is false. Waiting.");
                        return;
                    }
                }
            }
            else
            {
                Vector3 targetPosition = foundUnit.PositionWithoutType;
                float targetInteractDistance = foundUnit.InteractDistance;
                GoToTask.ToPositionAndIntecractWithNpc(targetPosition, _talkToUnitModel.UnitId, _talkToUnitModel.GossipIndex);
                if (myPosition.DistanceTo(targetPosition) < targetInteractDistance
                    && EvaluateCompleteCondition())
                {
                    IsCompleted = true;
                    return;
                }
            }
        }
    }
}
