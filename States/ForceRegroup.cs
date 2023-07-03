using robotManager.FiniteStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class ForceRegroup : State
    {
        public override string DisplayName => "Forcing regroup at entrance";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private robotManager.Helpful.Timer _gnomereganTimer = null;
        private int _gnomereganTimerTime = 120 * 1000;

        public ForceRegroup(
            ICache iCache,
            IEntityCache EntityCache,
            IProfileManager profilemanager)
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
                    || !_entityCache.Me.IsValid
                    || _entityCache.Me.IsDead
                    || Fight.InFight
                    || _entityCache.EnemiesAttackingGroup.Length > 0
                    //|| _cache.LootRollShow
                    || !_profileManager.ProfileIsRunning
                    || _profileManager.CurrentDungeonProfile.CurrentStep is LeaveDungeonStep
                    || _entityCache.ListGroupMember.Count() == 0 && _entityCache.ListPartyMemberNames.Count() == 0) // Not if you're alone
                {
                    return false;
                }

                if (_profileManager.CurrentDungeonProfile.CurrentStep is RegroupStep)
                {
                    // Gnomeregan exception to match entrances
                    if (_profileManager.CurrentDungeonProfile.MapId == 90)
                    {
                        if (_gnomereganTimer == null)
                        {
                            Logger.Log($"Started Gnomeregan safety timer ({_gnomereganTimerTime} s)");
                            _gnomereganTimer = new robotManager.Helpful.Timer(_gnomereganTimerTime);
                        }
                        if (_gnomereganTimer.IsReady)
                        {
                            Logger.Log($"Gnomeregan safety TP out/in");
                            _gnomereganTimer = null;
                            return true;
                        }
                    }
                    return false;
                }
                _gnomereganTimer = null;

                if (_entityCache.ListGroupMember.Count() != _entityCache.ListPartyMemberNames.Count())
                {
                    foreach (string playerName in _entityCache.ListPartyMemberNames.Where(p => !_entityCache.ListGroupMember.Any(lgm => lgm.Name == p)))
                    {
                        Logger.Log($"{playerName} is missing! Teleporting out and back in to regroup.");
                    }

                    return true;
                }

                if (!_entityCache.ListGroupMember.Any(player => _rezzClasses.Contains(player.WoWClass) && !player.IsDead)
                    && !_rezzClasses.Contains(_entityCache.Me.WoWClass))
                {
                    Logger.Log($"No healer alive. Teleporting out and back in to regroup.");
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            MovementManager.StopMove();
            Thread.Sleep(3000);
            _profileManager.UnloadCurrentProfile();
            Lua.LuaDoString("LFGTeleport(true);");
            Thread.Sleep(5000);
            Lua.LuaDoString("LFGTeleport(false);");
            Thread.Sleep(5000);
            _profileManager.LoadProfile(true);
        }
    }
}
