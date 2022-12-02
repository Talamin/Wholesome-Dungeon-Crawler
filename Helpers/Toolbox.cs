using robotManager.Helpful;
using System.Linq;
using System;
using System.Threading;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow;

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

        /*
        public static (bool, Vector3)[] MassTraceLine(Vector3[] fromArray, Vector3[] toArray,
            CGWorldFrameHitFlags hitFlags = CGWorldFrameHitFlags.HitTestAllWhitoutLiquid)
        {
            if (fromArray.Length != toArray.Length || fromArray.Length <= 0)
            {
                Logging.WriteError("MassTraceLine fromArray and toArray differ in length or is zero.");
                //ErrorOut(fromArray.Length);
            }

            int arrayLength = fromArray.Length;

            // Allocate memory
            uint fromArrayAdr = Memory.WowMemory.AllocData.Get(4 * 3 * arrayLength);
            uint toArrayAdr = Memory.WowMemory.AllocData.Get(4 * 3 * arrayLength);
            uint didHitArrayAdr = Memory.WowMemory.AllocData.Get(arrayLength);
            uint hitArrayAdr = Memory.WowMemory.AllocData.Get(4 * 3 * arrayLength);
            if (fromArrayAdr <= 0 || toArrayAdr <= 0 || hitArrayAdr <= 0 || didHitArrayAdr <= 0)
            {
                Logging.WriteError("Failed to allocate memory for MassTraceLine");
                //ErrorOut(arrayLength);
            }

            // Prepare allocated memory
            Mem.WriteBytes(fromArrayAdr, Extension.ConcatBytes(
                fromArray.Select(vector => (vector + new Vector3(0, 0, 1.5f)).ToBytes()).ToArray()));
            Mem.WriteBytes(toArrayAdr, Extension.ConcatBytes(
                toArray.Select(vector => (vector + new Vector3(0, 0, 1.5f)).ToBytes()).ToArray()));
            Mem.WriteByteRepeat(hitArrayAdr, 0, 4 * 3 * arrayLength);
            // Code injection
            var asm = new[] {
                $"cmp byte [{Addresses.InGameAddress}], 0",
                $"je @out",
                "push ebx",
                "push edi",
                "push esi",
                "sub esp, 16",
                "xor esi, esi",
                "xor edi, edi",
                "lea ebx, [esp + 12]", // hitFactor
                "@mainLoop:",
                $"lea eax, [{fromArrayAdr} + esi]",
                $"lea ecx, [{toArrayAdr} + esi]",
                $"lea edx, [{hitArrayAdr} + esi]",
                "mov dword [ebx], 1065353216", // set hitFactor to 1.0f
                "sub esp, 8",
                "push 0",
                $"push {(int)hitFlags}",
                "push ebx",
                "push edx",
                "push ecx",
                "push eax",
                $"call {Addresses.TraceLineAddress}",
                "add esp, 32",
                $"mov byte [{didHitArrayAdr} + edi], al",
                "add edi, 1",
                "add esi, 12",
                $"cmp edi, {arrayLength}",
                "jl @mainLoop",
                "add esp, 16",
                "pop esi",
                "pop edi",
                "pop ebx",
                "@out:",
                "ret"
            };

            Memory.WowMemory.InjectAndExecute(asm);

            // Get result
            byte[] readDidHit = Mem.ReadBytes(didHitArrayAdr, (uint)arrayLength);
            byte[] readHitArray = Mem.ReadBytes(hitArrayAdr, (uint)(arrayLength * 4 * 3));

            // Clean up memory
            Memory.WowMemory.AllocData.Free(fromArrayAdr);
            Memory.WowMemory.AllocData.Free(toArrayAdr);
            Memory.WowMemory.AllocData.Free(didHitArrayAdr);
            Memory.WowMemory.AllocData.Free(hitArrayAdr);

            var result = new (bool, Vector3)[arrayLength];
            for (var i = 0; i < arrayLength; i++)
            {
                result[i] = (readDidHit[i] != 0, new Vector3(
                    BitConverter.ToSingle(readHitArray, i * 12 + 0),
                    BitConverter.ToSingle(readHitArray, i * 12 + 4),
                    BitConverter.ToSingle(readHitArray, i * 12 + 8)
                ));
            }

            return result;
        }
        */
    }
}
