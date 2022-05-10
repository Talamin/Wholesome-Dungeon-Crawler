using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class MoveAlongPathStep : Step
    {
        private MoveAlongPath _moveAlongPathModel;

        public MoveAlongPathStep(MoveAlongPath model)
        {
            _moveAlongPathModel = model;
        }

        public void Run()
        {
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(Target) < 5f)
            {
                IsCompleted = true;
                return true;
            }

            MovementManager.Go(WTPathFinder.PathFromClosestPoint(Path));
            /*
            if (!_movehelper.IsMovementThreadRunning || _movehelper.CurrentTarget.DistanceTo(_target) > 2)
            {
                //_movehelper.StartMoveAlongThread(PathFromClosestPoint(_path));
            }
            */
            return IsCompleted = false;
        }

    }
}
