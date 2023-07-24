using robotManager.Helpful;
using System.Collections.Generic;
using System.Threading;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Helpers
{
    public class Toolbox
    {
        public static List<DungeonModel> GetListAvailableDungeons()
        {
            List<DungeonModel> result = new List<DungeonModel>();
            string[] availableInstances = Lua.LuaDoString<string[]>($@"
                local result = {{}};
                local tableInstances = GetLFDChoiceInfo();
                for instanceId, instanceInfo in pairs(tableInstances) do
                    local instanceName = instanceInfo[1];
                    local numPlayers = instanceInfo[12];
                    local instanceDifficulty = instanceInfo[11]; -- 0 Normal, 1 Heroic
                    if IsLFGDungeonJoinable(instanceId) then
                        table.insert(result, instanceId .. ""$"" .. instanceName .. ""$"" .. numPlayers .. ""$"" .. instanceDifficulty)
                    end
                end
                return unpack(result);
            ");

            foreach (string instance in availableInstances)
            {
                string[] instanceInfo = instance.Split('$');
                int dungeonId = int.Parse(instanceInfo[0]);
                string dungeonName = instanceInfo[1];
                int numPlayers = int.Parse(instanceInfo[2]);
                int difficulty = int.Parse(instanceInfo[3]);
                string type = difficulty == 0 ? "Normal" : "Heroic";

                if (numPlayers > 5
                    && dungeonId != 32) // LBRS is considered 15 men in LUA
                    continue;

                DungeonModel model = Lists.AllDungeons.Find(dungeon => dungeon.DungeonId == dungeonId);
                if (model == null)
                {
                    Logger.LogError($"Couldn't find client dungeon {dungeonName} ({type}) with ID {dungeonId} in internal list (Lists.AllDungeons)");
                    continue;
                }
                result.Add(model);
            }

            return result;
        }

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
                xvec = xvec + player.PositionWT.X;
                yvec = yvec + player.PositionWT.Y;
                zvec = zvec + player.PositionWT.Z;

                counter++;
            }

            return new Vector3(xvec / counter, yvec / counter, zvec / counter);
        }

        /// <summary>
        /// Returns nodes at regular distance intervals along a path. Doesn't include starting point.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="distanceBetweenPoints"></param>
        /// <param name="maxDistance"></param>
        /// <returns>Nodes at regular distance intervals along a path</returns>
        public static List<Vector3> GetPointsAlongPath(
            List<Vector3> path,
            float distanceBetweenPoints,
            float maxDistance)
        {
            List<Vector3> result = new List<Vector3>();
            float remainder = 0f;
            float totalDistance = 0f;

            if (path.Count <= 0)
            {
                return result;
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 segmentStart = path[i];
                Vector3 segmentEnd = path[i + 1];
                float segmentLength = segmentStart.DistanceTo(segmentEnd);

                if (totalDistance > maxDistance) break;

                for (float offsetIndex = distanceBetweenPoints; offsetIndex < segmentLength; offsetIndex += distanceBetweenPoints)
                {
                    if (remainder > 0)
                    {
                        offsetIndex -= remainder;
                        remainder = 0;
                    }

                    if (offsetIndex + distanceBetweenPoints > segmentLength)
                    {
                        remainder = segmentLength - offsetIndex;
                    }

                    Vector3 vector = new Vector3(segmentEnd.X - segmentStart.X, segmentEnd.Y - segmentStart.Y, segmentEnd.Z - segmentStart.Z);
                    double c = System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
                    double a = offsetIndex / c;
                    Vector3 offset = new Vector3(segmentStart.X + vector.X * a, segmentStart.Y + vector.Y * a, segmentStart.Z + vector.Z * a);

                    totalDistance += distanceBetweenPoints;
                    if (totalDistance > maxDistance) break;
                    result.Add(offset);
                }
            }

            return result;
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

        public static void AnswerYesReadyCHeck()
        {
            Logger.LogOnce($"Answering yes to ready check.");
            Lua.LuaDoString("ReadyCheckFrameYesButton:Click();");
            Lua.LuaDoString("ConfirmReadyCheck(true);");
        }

        public static void DoGroupReadyCheck()
        {
            Logger.Log($"Initiating ready check.");
            Lua.LuaDoString($"SetRaidTarget('player', 0)");
            Lua.LuaDoString("DoReadyCheck();");
            Thread.Sleep(1000);
        }
    }
}
