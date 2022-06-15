using robotManager.Helpful;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache.Entity;
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

        public static Vector3 PointInMidOfGroup(IWoWPlayer[] group)
        {
            float xvec = 0, yvec = 0, zvec = 0;
            int counter = 0;

            foreach (IWoWUnit player in group)
            {
                xvec = xvec + player.PositionWithoutType.X;
                yvec = yvec + player.PositionWithoutType.Y;
                zvec = zvec + player.PositionWithoutType.Z;

                counter++;
            }

            return new Vector3(xvec / counter, yvec / counter, zvec / counter);
        }
    }
}
