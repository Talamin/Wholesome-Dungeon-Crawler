using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeDungeonCrawler.Helpers.TargetingHelper;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class DefendVioletHoldStep : Step
    {
        private DefendVioletHoldModel _defendVioletHoldModel;
        private readonly IEntityCache _entityCache;
        private Timer _stepTimer;
        private int _defendSpotRadius;
        private int _timeToWaitInMilliseconds;
        private bool _resetTimerOnCombat;
        private readonly List<ICachedWoWUnit> _lowPrioUnits = new List<ICachedWoWUnit>();

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }
        public double GetTimeLeft => _stepTimer == null || _stepTimer.IsReady ? 0 : _stepTimer.TimeLeft();

        public ICachedWoWUnit ShouldDefendAgainst => _entityCache.EnemyUnitsList
                .Where(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition) <= _defendSpotRadius
                    && !Lists.MobsToIgnoreDefendingVioletHold.Contains(unit.Entry)
                    && !_lowPrioUnits.Contains(unit)
                    && unit.Reaction <= wManager.Wow.Enums.Reaction.Hostile)
                .OrderBy(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition))
                .FirstOrDefault();
        public ICachedWoWUnit ShouldDefendPortal => _entityCache.InterestingUnitsList
                .Where(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition) <= _defendSpotRadius
                    && unit.Entry == 31011
                )
                .OrderBy(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition))
                .FirstOrDefault();

        public ICachedWoWUnit ShouldPullBosses => _entityCache.EnemyUnitsList
                .Where(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition) <= _defendSpotRadius
                    && Lists.MobsToIgnoreDefendingVioletHold.Contains(unit.Entry)
                    && unit.PositionWT.DistanceTo(Lists.VioletHoldBossPositions[unit.Entry]) > 1
                    && !_lowPrioUnits.Contains(unit)
                    && unit.Reaction <= wManager.Wow.Enums.Reaction.Hostile)
                .OrderBy(unit => unit.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition))
                .FirstOrDefault();

        public DefendVioletHoldStep(DefendVioletHoldModel defendVioletHoldModel, IEntityCache entityCache) : base(defendVioletHoldModel.CompleteCondition)
        {
            _defendVioletHoldModel = defendVioletHoldModel;
            _entityCache = entityCache;
            _defendSpotRadius = _defendVioletHoldModel.DefendSpotRadius;
            _defendSpotRadius = _defendSpotRadius < 5 ? 5 : _defendSpotRadius;
            _timeToWaitInMilliseconds = _defendVioletHoldModel.Timer * 1000;
            _resetTimerOnCombat = _defendVioletHoldModel.ResetTimerOnCombat;
            Name = _defendVioletHoldModel.Name;
            StepFaction = _defendVioletHoldModel.StepFaction;
            StepRole = _defendVioletHoldModel.StepRole;
            PreEvaluationPass = EvaluateFactionCompletion() && EvaluateRoleCompletion();
        }

        public override void Initialize()
        {
            FightEvents.OnFightEnd += OnFightEnd;
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= OnFightEnd;
        }

        private void OnFightEnd(ulong guid)
        {
            if (_stepTimer != null && _resetTimerOnCombat)
            {
                _stepTimer.Reset();
            }
        }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

            if (_stepTimer == null)
            {
                _stepTimer = new Timer(_timeToWaitInMilliseconds);
            }
                               
            ICachedWoWUnit unitToAttack = ShouldDefendAgainst;
            if (unitToAttack != null)
            {
                Logger.Log($"Defending VH against {unitToAttack.Name}");
                ObjectManager.Me.Target = unitToAttack.Guid;
                Fight.StartFight(unitToAttack.Guid, false);
                return;
            }

            // move to portal
            ICachedWoWUnit portal = ShouldDefendPortal;
            if (portal != null && !MovementManager.InMovement
                && _entityCache.Me.PositionWT.DistanceTo(portal.PositionWT) > 5f)
            {
                Logger.Log($"Moving to portal {portal.Name}");
                List<Vector3> pathToCenter = PathFinder.FindPath(_entityCache.Me.PositionWT, portal.PositionWT);
                MovementManager.Go(pathToCenter);
                return;
            }

            // move to defend spot
            if (!MovementManager.InMovement
                && _entityCache.Me.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition) > 5f)
            {
                List<Vector3> pathToCenter = PathFinder.FindPath(_entityCache.Me.PositionWT, _defendVioletHoldModel.DefendPosition);
                MovementManager.Go(pathToCenter);
            }

            unitToAttack = ShouldPullBosses;
            if (unitToAttack != null)
            {
                Logger.Log($"Defending VH against boss {unitToAttack.Name}");
                ObjectManager.Me.Target = unitToAttack.Guid;
                Fight.StartFight(unitToAttack.Guid, false);
                return;
            }
            
            // end step
            if (_entityCache.Me.PositionWT.DistanceTo(_defendVioletHoldModel.DefendPosition) <= _defendSpotRadius
                && (_stepTimer.IsReady || EvaluateCompleteCondition()))
            {
                MarkAsCompleted();
                return;
            }
        }
    }
}
