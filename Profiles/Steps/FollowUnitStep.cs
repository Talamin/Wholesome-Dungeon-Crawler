using robotManager.Helpful;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class FollowUnitStep : Step
    {
        private FollowUnitModel _followUnitModel;
        private readonly IEntityCache _entityCache;
        private WoWUnit _unitToEscort;

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }

        public FollowUnitStep(FollowUnitModel followUnitModel, IEntityCache entityCache) : base(followUnitModel.CompleteCondition)
        {
            _followUnitModel = followUnitModel;
            _entityCache = entityCache;
            Name = followUnitModel.Name;
            StepFaction = followUnitModel.StepFaction;
            StepRole = followUnitModel.StepRole;
            PreEvaluationPass = EvaluateFactionCompletion() && EvaluateRoleCompletion();
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

            _unitToEscort = ObjectManager.GetObjectWoWUnit()
                .FirstOrDefault(unit =>
                    unit.IsAlive
                    && unit.Entry == _followUnitModel.UnitId);
            Vector3 myPosition = _entityCache.Me.PositionWT;

            if (_unitToEscort == null)
            {
                if (myPosition.DistanceTo(_followUnitModel.ExpectedStartPosition) >= 15)
                {
                    GoToTask.ToPosition(_followUnitModel.ExpectedStartPosition, 3.5f, false, context => IsCompleted);
                }
                else
                {
                    if (_followUnitModel.SkipIfNotFound && EvaluateCompleteCondition())
                    {
                        Logger.Log($"[Step {_followUnitModel.Name}]: Skipping. Unit {_followUnitModel.UnitId} is not here or condition is complete.");
                        MarkAsCompleted();
                        return;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Logger.Log($"[Step {_followUnitModel.Name}]: Unit {_followUnitModel.UnitId} is not around and SkipIfNotFound is false. Waiting.");
                        return;
                    }
                }
            }
            else
            {
                Vector3 escortPosition = _unitToEscort.PositionWithoutType;
                if (escortPosition.DistanceTo(_followUnitModel.ExpectedEndPosition) < 15
                    && EvaluateCompleteCondition())
                {
                    Logger.Log($"[Step {_followUnitModel.Name}]: {_unitToEscort.Name} has reached their destination");
                    MarkAsCompleted();
                    return;
                }

                IWoWUnit unitToDefendAgainst = ShouldDefendAgainst();
                if (unitToDefendAgainst != null)
                {
                    Logger.Log($"Defending {_unitToEscort.Name} against {unitToDefendAgainst.Name}");
                    ObjectManager.Me.Target = unitToDefendAgainst.Guid;
                    Fight.StartFight(unitToDefendAgainst.Guid, false);
                }

                if (!MovementManager.InMovement &&
                    _entityCache.Me.PositionWT.DistanceTo(escortPosition) > 15)
                {
                    GoToTask.ToPosition(escortPosition);
                }
            }
        }

        public IWoWUnit ShouldDefendAgainst()
        {
            if (_unitToEscort == null) return null;
            foreach (IWoWUnit unit in _entityCache.EnemyUnitsList)
            {
                if (unit.TargetGuid > 0
                    && unit.TargetGuid == _unitToEscort.Guid
                    && !Lists.MobsToIgnoreDuringSteps.Contains(unit.Entry))
                {
                    Logger.Log($"Defending Follow Unit against {unit.Name}");
                    ObjectManager.Me.Target = unit.Guid;
                    Fight.StartFight(unit.Guid, false);
                }
            }
            return null;
        }
    }
}
