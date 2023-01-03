using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class Dead : State, IState
    {
        public override string DisplayName => "Dead";

        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public Dead(IEntityCache iEntityCache, IProfileManager profilemanager)
        {
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
        }

        private List<WoWClass> _rezzClasses = new List<WoWClass> { WoWClass.Druid, WoWClass.Paladin, WoWClass.Priest, WoWClass.Shaman };
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.IsValid)
                {
                    return false;
                }

                return _entityCache.Me.IsDead;
            }
        }

        public override void Run()
        {
            if (WTLuaFrames.GetStaticPopup1Text().Contains("wants to resurrect you"))
            {
                Logger.LogOnce("Accepting resurrection from a player");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                return;
            }

            if (_entityCache.Me.Auras.ContainsKey(20762)) // Soulstone
            {
                Logger.LogOnce("Accepting soulstone resurrection");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                return;
            }

            if (_entityCache.Me.Auras.ContainsKey(8326)) // Ghost
            {
                if (MovementManager.InMovement)
                {
                    return;
                }

                if (_profileManager.CurrentDungeonProfile?.DeathRunPath != null
                    && _profileManager.CurrentDungeonProfile.DeathRunPath.Count > 0)
                {
                    Logger.Log("Running profile's death run");
                    List<Vector3> adjustedDeathPath = WTPathFinder.PathFromClosestPoint(_profileManager.CurrentDungeonProfile.DeathRunPath);
                    MovementManager.Go(adjustedDeathPath, false);
                }
                else
                {
                    var dungeon = Lists.AllDungeons.Where(x => x.ContinentId == Usefuls.ContinentId)
                            .OrderBy(y =>y.EntranceLoc.DistanceTo(_entityCache.Me.PositionCorpse))
                            .FirstOrDefault();
                    Logger.Log("No profile death run found, using pathfinder.");
                    GoToTask.ToPosition(dungeon.EntranceLoc, skipIfCannotMakePath: false);
                }
            }
            else
            {
                MovementManager.StopMove();
                if (_entityCache.ListGroupMember
                    .Any(player => _rezzClasses.Contains(player.WoWClass)
                        && !player.IsDead
                        && _entityCache.Me.PositionWithoutType.DistanceTo(player.PositionWithoutType) < 50))
                {
                    Logger.LogOnce("A group member can resurrect me. Waiting.");
                    Thread.Sleep(3000);
                }
                else
                {
                    Thread.Sleep(1000);
                    Logger.Log("Nothing can resurrect me. We will have to walk.");
                    Lua.LuaDoString("RepopMe();");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
