using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class Dead : State, IState
    {
        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private List<Vector3> _deathrun = new List<Vector3>();

        public Dead(ICache iCache, IEntityCache iEntityCache, IProfileManager profilemanager, int priority)
        {
            _cache = iCache;
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
            Priority = priority;
        }

        private List<WoWClass> _rezzClasses = new List<WoWClass> { WoWClass.Druid, WoWClass.Paladin, WoWClass.Priest, WoWClass.Shaman };
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.Valid)
                //|| _profileManager.CurrentDungeonProfile == null
                //|| _profileManager.CurrentDungeonProfile.CurrentStep == null)
                {
                    return false;
                }

                return _entityCache.Me.Dead;
            }
        }

        public override void Run()
        {
            //Notes: 
            //First: Check if we can  Selfrezz
            //Second: Check if a Character is around which can rezz and is alive
            //Third: Release for  Deathrun
            //Find out in which Dungeon we died.
            if (_cache.HaveResurrection)
            {
                Lua.LuaDoString("StaticPopup1Button1:Click()");
                Logger.Log("Accepted Resurrection");
            }

            if (_cache.IsInInstance)
            {
                if (_entityCache.Me.Auras.ContainsKey(20762))//Soulstonebuff
                {
                    Lua.LuaDoString("StaticPopup1Button1:Click()");
                    Logger.Log("SelfRezz progressed");
                }
                if (!_entityCache.ListGroupMember.Any(y => _rezzClasses.Contains(y.WoWClass) && !y.Dead && _entityCache.Me.PositionWithoutType.DistanceTo(y.PositionWithoutType) < 50))
                {
                    Logger.Log("No one to Rezz around, we have to use our Feet and walk back");
                    Lua.LuaDoString("RepopMe();");
                }
            }


            if (!_cache.IsInInstance)
            {
                if(_profileManager.CurrentDungeonProfile.DeathRunPathList != null)
                _deathrun = _profileManager.CurrentDungeonProfile.DeathRunPathList;
                if (_profileManager.CurrentDungeonProfile.DeathRunPathList != null && _profileManager.CurrentDungeonProfile.DeathRunPathList.Count > 0)
                    MovementManager.Go(_deathrun);
                else
                {
                    if (!MovementManager.InMovement)
                    {
                        Logger.Log("No Deathrun route found, using pathfinder.");
                        var dungeon = Lists.AllDungeons.Where(x => x.MapId == _profileManager.CurrentDungeonProfile.MapId).FirstOrDefault();
                        GoToTask.ToPosition(dungeon.EntranceLoc, skipIfCannotMakePath: false);
                    }
                }
            }
        }
    }
}
