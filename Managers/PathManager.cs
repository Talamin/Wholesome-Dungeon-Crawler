using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    internal class PathManager : IPathManager
    {
        private Vector3 _nextPathNode;
        private List<Vector3> _neighboringNodes;

        public void Initialize()
        {
            if (!Radar3D.IsLaunched) Radar3D.Pulse();
            Radar3D.OnDrawEvent += DrawEventPathManager;
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= DrawEventPathManager;
            Radar3D.Stop();
        }

        public void SetNextNode(Vector3 nextNode)
        {
            _nextPathNode = nextNode;
        }

        public void SetNeighboringNodes(List<Vector3> neighboringNodes)
        {
            _neighboringNodes = neighboringNodes;
        }

        private void DrawEventPathManager()
        {
            if (_nextPathNode != null)
            {
                Radar3D.DrawCircle(_nextPathNode, 0.4f, Color.Blue, true, 200);
            }

            if (_neighboringNodes != null)
            {
                foreach (Vector3 node in _neighboringNodes)
                {
                    if (_nextPathNode != null && node == _nextPathNode)
                    {
                        continue;
                    }
                    Radar3D.DrawCircle(node, 0.6f, Color.LawnGreen, false, 50);
                }
            }
        }
    }
}
