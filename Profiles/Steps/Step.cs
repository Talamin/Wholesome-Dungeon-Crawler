using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public abstract class Step : IStep
    {
        public bool IsCompleted { get; protected set; }
        public abstract string Name { get; }
        public abstract int Order { get; }

        public abstract void Run();

        public bool EvaluateCompleteCondition(StepCompleteConditionModel stepCompleteCondition)
        {
            if (stepCompleteCondition == null)
            {
                // No step condition
                return true;
            }

            switch (stepCompleteCondition.ConditionType)
            {
                case CompleteConditionType.Csharp:
                    return true;
                case CompleteConditionType.FlagsChanged:
                    WoWGameObject obj = ObjectManager.GetWoWGameObjectByEntry(stepCompleteCondition.GameObjectId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    bool resultFlagsChanged = obj.FlagsInt != stepCompleteCondition.InitialFlags;
                    if (resultFlagsChanged) Logger.Log($"[Condition PASS] {obj.Name}'s flags have changed from {stepCompleteCondition.InitialFlags} to {obj.FlagsInt}");
                    else Logger.Log($"[Condition FAIL] {obj.Name}'s flags haven't changed ({obj.FlagsInt})");
                    return resultFlagsChanged;
                case CompleteConditionType.HaveItem:
                    bool resultHaveItem = ItemsManager.GetItemCountByIdLUA((uint)stepCompleteCondition.ItemId) > 0;
                    if (resultHaveItem) Logger.Log($"[Condition PASS] You have item {stepCompleteCondition.ItemId} in your bags");
                    else Logger.Log($"[Condition FAIL] You don't have item {stepCompleteCondition.ItemId} in your bags");
                    return resultHaveItem;
                case CompleteConditionType.MobDead:
                    WoWUnit deadmob = ObjectManager.GetObjectWoWUnit()
                        .Where(x => x.Entry == stepCompleteCondition.DeadMobId)
                        .OrderBy(x => x.GetDistance)
                        .FirstOrDefault();
                    bool resultMobDead = deadmob == null || deadmob.IsDead;
                    if (resultMobDead) Logger.Log($"[Condition PASS] Unit {stepCompleteCondition.DeadMobId} is dead or absent");
                    else Logger.Log($"[Condition FAIL] Unit {stepCompleteCondition.DeadMobId} is not dead or absent");
                    return resultMobDead;
                case CompleteConditionType.MobPosition:
                    WoWUnit unitToCheck = ObjectManager
                        .GetWoWUnitByEntry(stepCompleteCondition.MobPositionId)
                        .OrderBy(x => x.GetDistance)
                        .FirstOrDefault();
                    Vector3 mobvec = stepCompleteCondition.MobPositionVector;
                    bool resultMobPosition = unitToCheck != null && mobvec.DistanceTo(unitToCheck.Position) < 5;
                    if (resultMobPosition) Logger.Log($"[Condition PASS] Unit {unitToCheck.Name} is at the expected position");
                    else Logger.Log($"[Condition FAIL] Unit {stepCompleteCondition.MobPositionId} is not at the expect position or absent");
                    return resultMobPosition;
                case CompleteConditionType.CantGossip:
                    WoWUnit gossipUnit = ObjectManager.GetObjectWoWUnit()
                        .Where(x => x.Entry == stepCompleteCondition.MobId)
                        .FirstOrDefault();
                    bool resultGossipUnit = gossipUnit != null && !gossipUnit.UnitNPCFlags.HasFlag(UnitNPCFlags.Gossip);
                    if (resultGossipUnit) Logger.Log($"[Condition PASS] Unit {stepCompleteCondition.MobId} can gossip");
                    else Logger.Log($"[Condition FAIL] Unit {stepCompleteCondition.MobId} can't gossip or is absent");
                    return resultGossipUnit;
                case CompleteConditionType.LOSCheck:
                    if (stepCompleteCondition.LOSPositionVectorFrom == null || stepCompleteCondition.LOSPositionVectorTo == null)
                    {
                        Logger.LogError($"[Condition FAIL] LoS check is missing a vector!");
                        return false;
                    }
                    // We first use a random LoS check (further than 1.5 yards) to reset the LoS cache
                    Vector3 resetLoSFrom = new Vector3(
                        stepCompleteCondition.LOSPositionVectorFrom.X + 10,
                        stepCompleteCondition.LOSPositionVectorFrom.Y + 10,
                        stepCompleteCondition.LOSPositionVectorFrom.Z + 10);
                    Vector3 resetLoSTo = new Vector3(
                        stepCompleteCondition.LOSPositionVectorTo.X + 10,
                        stepCompleteCondition.LOSPositionVectorTo.Y + 10,
                        stepCompleteCondition.LOSPositionVectorTo.Z + 10);
                    bool resetLoS = TraceLine.TraceLineGo(resetLoSFrom, resetLoSTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);

                    // Actual check
                    bool losResult = !TraceLine.TraceLineGo(stepCompleteCondition.LOSPositionVectorFrom, stepCompleteCondition.LOSPositionVectorTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                    if (losResult) Logger.Log($"[Condition PASS] LoS result is positive");
                    else Logger.Log($"[Condition FAIL] LoS result is negative");
                    return losResult;
                default:
                    return true;
            }
        }

        public void MarkAsCompleted() => IsCompleted = true;
    }
}
