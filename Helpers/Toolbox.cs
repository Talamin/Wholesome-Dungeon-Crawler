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
            Logger.Log($"Leaving party and teleporting out of dungeon");
            Lua.LuaDoString("LeaveParty();");
            Thread.Sleep(500);
            Lua.LuaDoString("LFGTeleport(true);");
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

        public static bool MemberHasRaidTarget(int targetIndex)
        {
            return Lua.LuaDoString<bool>($@"
                local nbPartyMembers = GetRealNumPartyMembers();
                local myName, _ = UnitName('player');
                local members = {{myName}};

                for index = 1, nbPartyMembers do
                    local playerName, _ = UnitName('party' .. index);
                    table.insert(members, playerName);
                end

                for k, v in pairs(members) do
                    if GetRaidTargetIndex(v) == {targetIndex} then
                        return true;
                    end
                end

                return false;
            ");
        }
        /*
        private static Vector3 _toLast = new Vector3();
        private static Vector3 _fromLast = new Vector3();
        private static bool _lastResult = true;

        public static bool CheckLos(Vector3 from, Vector3 to, CGWorldFrameHitFlags hitFlags = CGWorldFrameHitFlags.HitTestAll)
        {
            try
            {
                if (from.X != 0 && from.Y != 0 && to.X != 0 && to.Y != 0)
                {
                    // cache:
                    if (_toLast.DistanceTo(to) < 1.5f && _fromLast.DistanceTo(from) < 1.5f)
                    {
                        return _lastResult;
                    }
                    _toLast = to;
                    _fromLast = from;

                    uint end = Memory.WowMemory.Memory.AllocateMemory(0x4 * 3);
                    uint start = Memory.WowMemory.Memory.AllocateMemory(0x4 * 3);
                    uint result = Memory.WowMemory.Memory.AllocateMemory(0x4 * 3);
                    uint distance = Memory.WowMemory.Memory.AllocateMemory(0x4);
                    uint optional = Memory.WowMemory.Memory.AllocateMemory(0x4 * 3);
                    uint resultRet = Memory.WowMemory.Memory.AllocateMemory(0x4);


                    if (end <= 0 || start <= 0 || result <= 0 || distance <= 0 || optional <= 0)
                        return false;

                    Memory.WowMemory.Memory.WriteFloat(optional, 0);
                    Memory.WowMemory.Memory.WriteFloat(optional + 0x4, 0);
                    Memory.WowMemory.Memory.WriteFloat(optional + 0x8, 0);

                    Memory.WowMemory.Memory.WriteFloat(distance, 0.9f);

                    Memory.WowMemory.Memory.WriteFloat(result, 0);
                    Memory.WowMemory.Memory.WriteFloat(result + 0x4, 0);
                    Memory.WowMemory.Memory.WriteFloat(result + 0x8, 0);

                    Memory.WowMemory.Memory.WriteFloat(start, from.X);
                    Memory.WowMemory.Memory.WriteFloat(start + 0x4, from.Y);
                    Memory.WowMemory.Memory.WriteFloat(start + 0x8, from.Z + 1.5f);

                    Memory.WowMemory.Memory.WriteFloat(end, to.X);
                    Memory.WowMemory.Memory.WriteFloat(end + 0x4, to.Y);
                    Memory.WowMemory.Memory.WriteFloat(end + 0x8, to.Z + 1.5f);
                    Memory.WowMemory.Memory.WriteInt32(resultRet, 0);

                    string[] asm = new[]
                    {
                        "push " + 0,
                        "push " + (uint) hitFlags,
                        "push " + distance,
                        "push " + result,
                        "push " + start,
                        "push " + end,
                        //"call " + (Memory.WowProcess.WowModule + (uint) Addresses.FunctionWow.CGWorldFrame__Intersect),
                        "call " + (Memory.WowMemory.Memory.GetProcess().MainModule.BaseAddress.ToInt64() + (uint) 0x93BACE),
                        "mov [" + resultRet + "], al",
                        "add esp, " + (uint) 0x18,
                        "@out:",
                        "retn"
                    };

                    Memory.WowMemory.InjectAndExecute(asm);
                    bool ret = Memory.WowMemory.Memory.ReadInt32(resultRet) > 0;

                    Memory.WowMemory.Memory.FreeMemory(resultRet);
                    Memory.WowMemory.Memory.FreeMemory(end);
                    Memory.WowMemory.Memory.FreeMemory(start);
                    Memory.WowMemory.Memory.FreeMemory(result);
                    Memory.WowMemory.Memory.FreeMemory(distance);
                    Memory.WowMemory.Memory.FreeMemory(optional);

                    _lastResult = ret;
                    return ret;
                }
                return true;
            }
            catch (Exception exception)
            {
                Logging.WriteError(
                    "TraceLineGo(Point from, Point to, Enums.CGWorldFrameHitFlags hitFlags = Enums.CGWorldFrameHitFlags.HitTestAll): " +
                    exception);
                return true;
            }
        }*/
    }
}
