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
            Radar3D.OnDrawEvent += DrawEvent;
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= DrawEvent;
        }

        public void SetNextNode(Vector3 nextNode)
        {
            _nextPathNode = nextNode;
        }

        public void SetNeighboringNodes(List<Vector3> neighboringNodes)
        {
            _neighboringNodes = neighboringNodes;
        }

        private void DrawEvent()
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
