using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class ForceRegroup : State
    {
        public override string DisplayName => "Leaving and Returning to Dungeon for Regroup";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;

        public ForceRegroup(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
        }

        private List<WoWClass> _rezzClasses = new List<WoWClass> { WoWClass.Druid, WoWClass.Paladin, WoWClass.Priest, WoWClass.Shaman };

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_cache.IsInInstance
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.Dead
                    || Fight.InFight
                    || _cache.LootRollShow
                    || _profileManager.CurrentDungeonProfile == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep == null
                    || _profileManager.CurrentDungeonProfile.CurrentStep.Order <= 0
                    // checks if heal is around and near us, or if I am heal
                    || _rezzClasses.Contains(_entityCache.Me.WoWClass)
                    || _entityCache.ListGroupMember.Any(player =>
                        _rezzClasses.Contains(player.WoWClass)))
                {
                    return false;
                }

                //Checks if 1 or more dead + no healer alive or in sight --> teleport out/in and reset the profile
                return _entityCache.ListGroupMember.Count() != _entityCache.ListPartyMemberNames.Count()
                    || _entityCache.ListGroupMember.Any(member => member.Dead);
            }
        }


        public override void Run()
        {
            Logger.Log($"Group died, need to Regroup, leaving Dungeon");
            Lua.LuaDoString("LFGTeleport(true);");
            Thread.Sleep(10000);
            Logger.Log($"Returning to dungeon");
            Lua.LuaDoString("LFGTeleport(false);");
            Thread.Sleep(10000);
        }
    }
}
