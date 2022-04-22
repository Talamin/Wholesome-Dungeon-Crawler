﻿using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class GoTo : Step
    {
        private readonly float _precision;
        private readonly float _randomizeEnd;
        private readonly float _randomization;
        private readonly Vector3 _targetPosition;

        public GoTo(Vector3 targetPosition, string stepName = "GoTo", float precision = 2f, float randomizeEnd = 0, float randomization = 0) : base(stepName)
        {
            _targetPosition = targetPosition;
            _precision = precision;
            _randomizeEnd = randomizeEnd;
            _randomization = randomization;
        }

        public override bool Pulse()
        {
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(_targetPosition) < _precision)
            {
                IsCompleted = true;
                return true;
            }

            if (!MoveHelper.IsMovementThreadRunning || MoveHelper.CurrentTarget.DistanceTo(_targetPosition) > _precision)
            {
                MoveHelper.StartGoToThread(_targetPosition);
            }

            return IsCompleted;
        }
    }
}
