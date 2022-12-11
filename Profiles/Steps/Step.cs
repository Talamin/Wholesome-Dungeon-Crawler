using robotManager.Helpful;
using System;
using System.Linq;
using System.Web;
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
        private string _lastLog;

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
                    if (resultFlagsChanged) LogUnique($"[Condition PASS] {obj.Name}'s flags have changed from {stepCompleteCondition.InitialFlags} to {obj.FlagsInt}");
                    else LogUnique($"[Condition FAIL] {obj.Name}'s flags haven't changed ({obj.FlagsInt})");
                    return resultFlagsChanged;
                case CompleteConditionType.HaveItem:
                    bool resultHaveItem = ItemsManager.GetItemCountByIdLUA((uint)stepCompleteCondition.ItemId) > 0;
                    if (resultHaveItem) LogUnique($"[Condition PASS] You have item {stepCompleteCondition.ItemId} in your bags");
                    else LogUnique($"[Condition FAIL] You don't have item {stepCompleteCondition.ItemId} in your bags");
                    return resultHaveItem;
                case CompleteConditionType.MobDead:
                    WoWUnit deadmob = ObjectManager.GetObjectWoWUnit()
                        .Where(x => x.Entry == stepCompleteCondition.DeadMobId)
                        .OrderBy(x => x.GetDistance)
                        .FirstOrDefault();
                    bool resultMobDead = deadmob == null || deadmob.IsDead;
                    if (resultMobDead) LogUnique($"[Condition PASS] Unit {stepCompleteCondition.DeadMobId} is dead or absent");
                    else LogUnique($"[Condition FAIL] Unit {stepCompleteCondition.DeadMobId} is not dead or absent");
                    return resultMobDead;
                case CompleteConditionType.MobPosition:
                    WoWUnit unitToCheck = ObjectManager
                        .GetWoWUnitByEntry(stepCompleteCondition.MobPositionId)
                        .OrderBy(x => x.GetDistance)
                        .FirstOrDefault();
                    Vector3 mobvec = stepCompleteCondition.MobPositionVector;
                    bool resultMobPosition = unitToCheck != null && mobvec.DistanceTo(unitToCheck.Position) < 5;
                    if (resultMobPosition) LogUnique($"[Condition PASS] Unit {unitToCheck.Name} is at the expected position");
                    else LogUnique($"[Condition FAIL] Unit {stepCompleteCondition.MobPositionId} is not at the expect position or absent");
                    return resultMobPosition;
                case CompleteConditionType.CantGossip:
                    WoWUnit gossipUnit = ObjectManager.GetObjectWoWUnit()
                        .Where(x => x.Entry == stepCompleteCondition.MobId)
                        .FirstOrDefault();
                    bool resultGossipUnit = gossipUnit != null && !gossipUnit.UnitNPCFlags.HasFlag(UnitNPCFlags.Gossip);
                    if (resultGossipUnit) LogUnique($"[Condition PASS] Unit {stepCompleteCondition.MobId} can gossip");
                    else LogUnique($"[Condition FAIL] Unit {stepCompleteCondition.MobId} can't gossip or is absent");
                    return resultGossipUnit;
                case CompleteConditionType.LOSCheck:
                    if (stepCompleteCondition.LOSPositionVectorFrom == null || stepCompleteCondition.LOSPositionVectorTo == null)
                    {
                        Logger.LogError($"[Condition FAIL] LoS check is missing a vector!");
                        return false;
                    }
                    // We first use a random LoS check (further than 1.5 yards) to reset the LoS cache
                    Random rnd = new Random();
                    int offset = rnd.Next(5, 30);                    
                    Vector3 resetLoSFrom = new Vector3(
                        stepCompleteCondition.LOSPositionVectorFrom.X + offset,
                        stepCompleteCondition.LOSPositionVectorFrom.Y + offset,
                        stepCompleteCondition.LOSPositionVectorFrom.Z + offset);
                    Vector3 resetLoSTo = new Vector3(
                        stepCompleteCondition.LOSPositionVectorTo.X + offset,
                        stepCompleteCondition.LOSPositionVectorTo.Y + offset,
                        stepCompleteCondition.LOSPositionVectorTo.Z + offset);
                    bool resetLoS = TraceLine.TraceLineGo(resetLoSFrom, resetLoSTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);                    

                    // Actual check
                    bool losResult = !TraceLine.TraceLineGo(stepCompleteCondition.LOSPositionVectorFrom, stepCompleteCondition.LOSPositionVectorTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                    if (losResult) LogUnique($"[Condition PASS] LoS result is positive");
                    else LogUnique($"[Condition FAIL] LoS result is negative");
                    return losResult;
                default:
                    return true;
            }
        }

        public void MarkAsCompleted() => IsCompleted = true;

        private void LogUnique(string message)
        {
            if (_lastLog != message)
            {
                _lastLog = message;
                Logger.Log(message);
            }
        }
    }
}
