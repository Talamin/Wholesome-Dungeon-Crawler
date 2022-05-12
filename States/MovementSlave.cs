using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class MovementSlave : State
    {
        public override string DisplayName => "Slave";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public MovementSlave(ICache iCache, IEntityCache EntityCache, int priority)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            Priority = priority;
        }

        private int FollowRange = 10;
        private Vector3 oldleaderpos = new Vector3(0, 0, 0);
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    || !_entityCache.Me.Valid
                    || !_entityCache.Me.InCombatFlagOnly
                    || !_cache.IsInInstance
                    || Fight.InFight)
                {
                    return false;
                }

                if(_entityCache.Me.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                {
                    return false;
                }

                IWoWUnit Tank = _entityCache.ListGroupMember.Where(t => t.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName).FirstOrDefault();
                if(Tank == null)
                {
                    return false;
                }
                    
                if(WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == WholesomeDungeonCrawlerSettings.LFGRoles.RDPS)
                {
                    FollowRange = WholesomeDungeonCrawlerSettings.CurrentSetting.FollowRangeRDPS;
                }
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == WholesomeDungeonCrawlerSettings.LFGRoles.MDPS)
                {
                    FollowRange = WholesomeDungeonCrawlerSettings.CurrentSetting.FollowRangeMDPS;
                }
                if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == WholesomeDungeonCrawlerSettings.LFGRoles.Heal)
                {
                    FollowRange = WholesomeDungeonCrawlerSettings.CurrentSetting.FollowRangeHeal;
                }

                //Checks when not to follow
                //If we are moving, return false
                if (MovementManager.InMoveTo || MovementManager.InMovement)
                {
                    return false;
                }
                //////Making Preperations to possible Follow
                //checks if oldleaderpos isn´t set, mostly at the beginning of an Dungeon and the Position is not known
                if (oldleaderpos == new Vector3(0, 0, 0))
                {
                    //set oldleaderpos to actual leader Position
                    oldleaderpos =Tank.PositionWithoutType;
                }
                //Check if our Tank made 6 yards from his old position, then follow him
                if (Tank.PositionWithoutType.DistanceTo(oldleaderpos) > 6)
                {
                    oldleaderpos = Tank.PositionWithoutType;
                }
                //If we are in Range of our Tank
                if(_entityCache.Me.PositionWithoutType.DistanceTo(oldleaderpos) <= FollowRange)
                {
                    return false;
                }

                if (_entityCache.Me.PositionWithoutType.DistanceTo(oldleaderpos) > FollowRange
                    || TraceLine.TraceLineGo(Tank.PositionWithoutType))
                {
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            //Now we try to calculate if the Tank is behind a cliff or something, by comparing the distance for sight and real path lenght.
            //If we differ 5%, we have to consider that the real path is larger to avoid an obstacle, so we use pathfinder and navigate relatively close to him
            //Else we use direkt moveto

            IWoWUnit Tank = _entityCache.ListGroupMember.Where(t => t.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName).FirstOrDefault();
            //calculates real distance by using pathfinder
            float pathcalc = WholesomeToolbox.WTPathFinder.CalculatePathTotalDistance(_entityCache.Me.PositionWithoutType, Tank.PositionWithoutType);
            //calculates distance by sightdistance
            float sight = _entityCache.Me.PositionWithoutType.DistanceTo(Tank.PositionWithoutType);

            //Logger.Log($"Following State: Distance in sight: {sight} in pathcalc {pathcalc} Difference in pathcalc/sight {pathcalc / sight * 100}");

            //check for line of sight, if not use pathfinder until you can see the Target
            if (TraceLine.TraceLineGo(Tank.PositionWithoutType))
            {
                Logger.Log("Following State: We don´t have LOS to the Tank, so we start following");
                MovementManager.Go(PathFinder.FindPath(_entityCache.Me.PositionWithoutType, Tank.PositionWithoutType, false));
                return;
            }

            //check if the difference between calculated path and on sight is more then 5%, so we use pathfinder and navigate until we are near the half way of the follow state
            if ((pathcalc / sight) * 100 > 105)
            {
                if(_entityCache.Me.PositionWithoutType.DistanceTo(Tank.PositionWithoutType) >= (FollowRange / 2))
                {
                    Logger.Log("Following State: Leader is behind a Cliff, using Pathfinder to get along");
                    MovementManager.Go(PathFinder.FindPath(_entityCache.Me.PositionWithoutType, oldleaderpos, false));
                }
                return;
            }

            //check if the difference between calculated path and on sight is less then 5%, then we use the normal MoveAlong until we are in  Followrange
            if ((pathcalc / sight) * 100 <= 105)
            {
                if (_entityCache.Me.PositionWithoutType.DistanceTo(oldleaderpos) >= FollowRange)
                {
                    Logger.Log("Following State: Leader is out of Range, following normal");
                    MovementManager.Go(PathFinder.FindPath(_entityCache.Me.PositionWithoutType, oldleaderpos, false));
                }
            }
        }
    }
}
