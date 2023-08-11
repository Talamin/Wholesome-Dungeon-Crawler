using robotManager.FiniteStateMachine;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class DeadDive : State, IState
    {
        public override string DisplayName => "DeadDive";

        private readonly IEntityCache _entityCache;

        public DeadDive(IEntityCache iEntityCache)
        {
            _entityCache = iEntityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (_entityCache.Me.Swimming
                    || !_entityCache.Me.IsDead
                    || !MovementManager.InMovement
                    || !_entityCache.Me.Auras.ContainsKey(8326))
                {
                    return false;
                }
                
                return MovementManager.CurrentPath.Find(node => node == MovementManager.CurrentMoveTo) != null 
                    && MovementManager.CurrentPath.Find(node => node == MovementManager.CurrentMoveTo).Type == "Swimming";
            }
        }

        public override void Run()
        {
            Logger.Log("Diving!");
            MovementManager.StopMove();
            MovementManager.GoUnderWater();
            Thread.Sleep(1000);
            Lua.LuaDoString("SitStandOrDescendStart();");
            Thread.Sleep(500);
            Lua.LuaDoString("DescendStop();");
        }
    }
}
