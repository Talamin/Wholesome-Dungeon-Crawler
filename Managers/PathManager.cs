using robotManager.Helpful;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WholesomeDungeonCrawler.Helpers;
using wManager.Events;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    internal class PathManager : IPathManager
    {
        private List<Vector3> _currentProfilePath;
        public Vector3 NextPathNode { get; private set; }

        public void Initialize()
        {
            MovementEvents.OnMoveToPulse += OnMoveToPulse;
            Radar3D.OnDrawEvent += DrawEvent;
        }

        public void Dispose()
        {
            MovementEvents.OnMoveToPulse -= OnMoveToPulse;
            Radar3D.OnDrawEvent -= DrawEvent;
        }

        public void SetCurrentProfilePath(List<Vector3> path)
        {
            _currentProfilePath = path;
        }

        private void OnMoveToPulse(Vector3 point, CancelEventArgs cancelable)
        {
            if (_currentProfilePath != null)
            {
                if (point != NextPathNode && _currentProfilePath.Contains(point))
                {
                    NextPathNode = point;
                }
            }
        }

        private void DrawEvent()
        {
            if (_currentProfilePath != null && NextPathNode != null)
            {
                Radar3D.DrawCircle(NextPathNode, 0.4f, Color.Blue, true, 200);
                foreach (Vector3 node in MoveHelper.GetNodesAround(_currentProfilePath, NextPathNode, 2))
                {
                    Radar3D.DrawCircle(node, 0.5f, Color.GreenYellow, true, 50);
                }
            }
        }
    }
}
