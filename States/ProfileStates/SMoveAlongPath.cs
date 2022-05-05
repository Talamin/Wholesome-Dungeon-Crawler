using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States.ProfileStates
{
    class SMoveAlongPath : State
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfile _profile;
        public SMoveAlongPath(ICache iCache, IEntityCache iEntityCache, IProfile iprofile, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profile = iprofile;
            Priority = priority;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid
                    || Fight.InFight)
                {
                    return false;
                }

                return _profile.CurrentStepType.Contains("MoveAlongPath");
            }
        }

        public override void Run()
        {

            //List<Vector3> Path = _profile.CurrentState .CurrentStep.Path;

            //if (_entityCache.Me.PositionWithoutType.DistanceTo(_profile.CurrentStep.Path.Last()) < 5f)
            //{
            //    _profile.CurrentStep.IsCompleted = true;
            //    _profile.ExecuteSteps();
            //}

            //MovementManager.Go(WTPathFinder.PathFromClosestPoint(Path));
            ///*
            //if (!_movehelper.IsMovementThreadRunning || _movehelper.CurrentTarget.DistanceTo(_target) > 2)
            //{
            //    //_movehelper.StartMoveAlongThread(PathFromClosestPoint(_path));
            //}
            //*/

            //_profile.CurrentStep.IsCompleted = false;
            //_profile.ExecuteSteps();
        }
    }
}
