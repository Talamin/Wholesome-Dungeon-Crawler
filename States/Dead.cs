using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
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
                    || !_entityCache.Me.Valid)
                {
                    return false;
                }

                return _entityCache.Me.Dead;
            }
        }

        public override void Run()
        {
            if (WTLuaFrames.GetStaticPopup1Text().Contains("wants to resurrect you"))
            {
                Logger.Log("Accepting resurrection from a player");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                return;
            }

            if (_entityCache.Me.Auras.ContainsKey(20762)) // Soulstone
            {
                Logger.Log("Accepting soulstone resurrection");
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                return;
            }

            if (_entityCache.Me.Auras.ContainsKey(8326)) // Ghost
            {
                if (MovementManager.InMovement)
                {
                    return;
                }

                if (_profileManager.CurrentDungeonProfile?.DeathRunPathList != null
                    && _profileManager.CurrentDungeonProfile.DeathRunPathList.Count > 0)
                {
                    Logger.Log("Running profile's death run");
                    List<Vector3> adjustedDeathPath = WTPathFinder.PathFromClosestPoint(_profileManager.CurrentDungeonProfile.DeathRunPathList);
                    MovementManager.Go(adjustedDeathPath);
                }
                else
                {
                    Logger.Log("No profile death run found, using pathfinder.");
                    var dungeon = Lists.AllDungeons.Where(x => x.MapId == _profileManager.CurrentDungeonProfile.MapId).FirstOrDefault();
                    GoToTask.ToPosition(dungeon.EntranceLoc, skipIfCannotMakePath: false);
                }
            }

            if (_entityCache.ListGroupMember
                .Any(player => _rezzClasses.Contains(player.WoWClass)
                    && !player.Dead
                    && _entityCache.Me.PositionWithoutType.DistanceTo(player.PositionWithoutType) < 50))
            {
                Logger.Log("A group member can resurrect me. Waiting.");
                Thread.Sleep(3000);
            }
            else
            {
                Logger.Log("Nothing can resurrect me. We will have to walk.");
                Lua.LuaDoString("RepopMe();");
            }
        }
    }
}
