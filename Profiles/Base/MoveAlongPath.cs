using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Base
{
    internal class MoveAlongPath : Step
    {

        private readonly IMoveHelper _movehelper;

        public MoveAlongPath(IMoveHelper imovehelper)
        {
            _movehelper = imovehelper;
        }

        private readonly List<Vector3> _path;
        private readonly float _randomization;
        private readonly Vector3 _target;

        public MoveAlongPath(List<Vector3> path, string stepName = "MoveAlongPath", float randomization = 0) :
            base(stepName)
        {
            _path = path;
            _randomization = randomization;
            _target = path.LastOrDefault();
        }

        private static List<Vector3> PathFromClosestPoint(List<Vector3> path)
        {
            var sortingList = new Tuple<ushort, Vector3>[path.Count];

            for (ushort i = 0; i < path.Count; i++) sortingList[i] = new Tuple<ushort, Vector3>(i, path[i]);

            Vector3 myPosition = ObjectManager.Me.PositionWithoutType;
            ushort startIndex = sortingList.OrderBy(tuple => tuple.Item2.DistanceTo(myPosition))
                .FirstOrDefault(tuple => !TraceLine.TraceLineGo(tuple.Item2))?.Item1 ?? 0;

            if (startIndex != 0) Logger.LogDebug($"Skipped the first {startIndex} steps of the path.");

            return startIndex != 0 ? path.GetRange(startIndex, path.Count - startIndex) : path;
        }

        public override bool Pulse()
        {
            if (ObjectManager.Me.PositionWithoutType.DistanceTo(_target) < 5f)
            {
                IsCompleted = true;
                return true;
            }

            if (!_movehelper.IsMovementThreadRunning || _movehelper.CurrentTarget.DistanceTo(_target) > 2)
            {
                //_movehelper.StartMoveAlongThread(PathFromClosestPoint(_path));
            }

            return IsCompleted;
        }

    }
}
