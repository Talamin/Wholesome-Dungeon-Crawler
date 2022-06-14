using System.Threading;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Helpers
{
    public class Toolbox
    {
        public static void LeaveDungeonAndGroup()
        {
            Lua.LuaDoString("LFGTeleport(true);");
            Thread.Sleep(3000);
            Lua.LuaDoString("LeaveParty();");
            Thread.Sleep(3000);
        }

        public static void StopAllMoves()
        {
            MovementManager.StopMove();
            MovementManager.StopMoveNewThread();
            MovementManager.StopMoveOnly();
            MovementManager.StopMoveTo();
            MovementManager.StopMoveToNewThread();
        }
    }
}
