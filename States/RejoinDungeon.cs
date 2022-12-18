using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class RejoinDungeon : State
    {
        public override string DisplayName => "Rejoining Dungeon";

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private Timer _stateTimer = new Timer();

        public RejoinDungeon(ICache iCache, IEntityCache EntityCache, IProfileManager profilemanager)
        {
            _cache = iCache;
            _entityCache = EntityCache;
            _profileManager = profilemanager;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_stateTimer.IsReady
                    || !_entityCache.Me.Valid
                    || _entityCache.Me.Dead
                    || Fight.InFight
                    || _profileManager.CurrentDungeonProfile != null
                    || _cache.IsInInstance
                    || !Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon:IsVisible()"))
                {
                    return false;
                }

                _stateTimer = new Timer(3000);
                return true; ;
            }
        }


        public override void Run()
        {
            Logger.LogOnce($"Rejoining dungeon");
            MovementManager.StopMove();
            Thread.Sleep(1000);
            Lua.LuaDoString("LFGTeleport(false);");
            Thread.Sleep(5000);
        }
    }
}
