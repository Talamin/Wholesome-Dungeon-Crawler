/*using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class MovementSlaveZero : State
    {
        public override string DisplayName { get; set; } = "Follow path slave";

        private readonly int _followDistance;
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private readonly List<(IStep step, Vector3 node)> _entirePath = new List<(IStep step, Vector3 node)>();
        private readonly List<(IStep step, Vector3 node)> _pathToTank = new List<(IStep step, Vector3 node)>();
        private readonly List<(IStep step, Vector3 node)> _debugPath = new List<(IStep step, Vector3 node)>();

        public MovementSlaveZero(ICache iCache, IEntityCache entityCache, IProfileManager profileManager, int priority)
        {
            _cache = iCache;
            _entityCache = entityCache;
            _profileManager = profileManager;
            Priority = priority;
            switch (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole)
            {
                case LFGRoles.MDPS:
                    _followDistance = 10;
                    break;
                case LFGRoles.RDPS:
                    _followDistance = 20;
                    break;
                case LFGRoles.Heal:
                    _followDistance = 25;
                    break;
                case LFGRoles.Tank:
                    _followDistance = 5;
                    break;
            }
        }

        public void Initialize()
        {
            Radar3D.OnDrawEvent += Radar3DOnDrawEvent;
        }

        public void Dispose()
        {
            Radar3D.OnDrawEvent -= Radar3DOnDrawEvent;
        }

        public override bool NeedToRun
        {
            get
            {
                _entirePath.Clear();
                _debugPath.Clear();
                _pathToTank.Clear();
                DisplayName = "Follow path slave";

                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || _profileManager == null
                    || _profileManager.CurrentDungeonProfile == null
                    || _entityCache.TankUnit?.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) <= _followDistance)
                {
                    return false;
                }

                if (_cache.IAmTank)
                {
                    return false;
                }

                if (_entityCache.TankUnit == null)
                {
                    Logger.Log("No IWoWUnit Tankunit");
                    return false;
                }

                foreach (KeyValuePair<IStep, List<Vector3>> entry in _profileManager.CurrentDungeonProfile.DungeonPath)
                {
                    foreach (Vector3 node in entry.Value)
                    {
                        _entirePath.Add((entry.Key, node));
                    }
                }

                List<(IStep, Vector3)> closestNodesFromTank = _entirePath
                    .Where(tuple => tuple.node.DistanceTo(_entityCache.TankUnit.PositionWithoutType) < 15)
                    .OrderBy(tuple => tuple.node.DistanceTo(_entityCache.TankUnit.PositionWithoutType))
                    .ToList();

                List<(IStep, Vector3)> closestNodesFromMe = _entirePath
                    .Where(tuple => tuple.node.DistanceTo(_entityCache.Me.PositionWithoutType) < 50)
                    .OrderBy(tuple => tuple.node.DistanceTo(_entityCache.Me.PositionWithoutType))
                    .ToList();

                List<(IStep step, Vector3 node)> selectedTankNodes = new List<(IStep, Vector3)>();
                foreach ((IStep, Vector3) tuple in closestNodesFromTank)
                {
                    if (!selectedTankNodes.Exists(t => t.step == tuple.Item1))
                    {
                        selectedTankNodes.Add(tuple);
                    }
                }

                List<(IStep step, Vector3 node)> selectedPlayerNodes = new List<(IStep, Vector3)>();
                foreach ((IStep, Vector3) tuple in closestNodesFromMe)
                {
                    if (!selectedPlayerNodes.Exists(t => t.step == tuple.Item1))
                    {
                        selectedPlayerNodes.Add(tuple);
                    }
                }

                float totalDistance = float.MaxValue;
                List<(IStep, Vector3)> pathToTank = new List<(IStep, Vector3)>();

                foreach ((IStep, Vector3) tankNode in selectedTankNodes)
                {
                    foreach ((IStep, Vector3) myNode in selectedPlayerNodes)
                    {
                        int indexOfMyNode = _entirePath.FindIndex(node => node == myNode);
                        int indexOfTankNode = _entirePath.FindIndex(node => node == tankNode);

                        // ahead of the tank
                        if (indexOfMyNode > indexOfTankNode)
                        {
                            continue;
                        }

                        float thisPathTotalDistance = 0;
                        for (int i = indexOfMyNode; i < indexOfTankNode - 1; i++)
                        {
                            thisPathTotalDistance += _entirePath[i].node.DistanceTo(_entirePath[i + 1].node);
                            if (thisPathTotalDistance > totalDistance)
                            {
                                break;
                            }
                        }

                        if (thisPathTotalDistance < totalDistance)
                        {
                            totalDistance = thisPathTotalDistance;
                            pathToTank = _entirePath.GetRange(indexOfMyNode, indexOfTankNode - indexOfMyNode);
                        }
                    }
                }

                // get current path step
                if (pathToTank.Count > 0)
                {
                    _pathToTank.AddRange(pathToTank);
                    DisplayName = $"Following path towards {_entityCache.TankUnit.Name} on step path {_pathToTank[0].step.Name}";
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            List<Vector3> pathToFollow = new List<Vector3>();
            foreach ((IStep, Vector3) tuple in _pathToTank)
            {
                pathToFollow.Add(tuple.Item2);
            }

            if (_entityCache.TankUnit != null
                && _entityCache.TankUnit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) > _followDistance)
            {
                if (_pathToTank.Count > 1)
                {
                    // rejoin path using pathfinder
                    GoToTask.ToPosition(pathToFollow[0]);
                    if (_entityCache.Me.PositionWithoutType.DistanceTo(pathToFollow[0]) < 8)
                    {
                        MovementManager.Go(pathToFollow);
                        while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                            && MovementManager.InMovement 
                            && _entityCache.TankUnit.PositionWithoutType.DistanceTo(_entityCache.Me.PositionWithoutType) > _followDistance)
                        {
                            Thread.Sleep(500);
                        }
                        MovementManager.StopMove();
                    }
                }
                else
                {
                    GoToTask.ToPosition(pathToFollow[0]);
                }
            }
        }

        private void Radar3DOnDrawEvent()
        {
            if (_profileManager == null || _profileManager.CurrentDungeonProfile == null)
            {
                return;
            }

            if (_entirePath.Count > 0)
            {
                for (int i = 0; i < _entirePath.Count - 1; i++)
                {
                    Radar3D.DrawLine(_entirePath[i].node, _entirePath[i + 1].node, Color.Blue, 75);
                }
            }

            if (_pathToTank.Count > 0)
            {
                for (int i = 0; i < _pathToTank.Count - 1; i++)
                {
                    Radar3D.DrawLine(_pathToTank[i].node, _pathToTank[i + 1].node, Color.White);
                }
            }

            if (_debugPath.Count > 0)
            {
                for (int i = 0; i < _debugPath.Count - 1; i++)
                {
                    Radar3D.DrawLine(_debugPath[i].node, _debugPath[i + 1].node, Color.Yellow, 125);
                }
            }
        }
    }
}
*/