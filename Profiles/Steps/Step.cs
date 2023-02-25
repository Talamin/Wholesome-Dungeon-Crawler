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
                case CompleteConditionType.MobAttackable:
                    WoWUnit unitAttackable = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == stepCompleteCondition.MobAttackableEntry)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    // Unit missing or dead
                    if (unitAttackable == null || unitAttackable.IsDead)
                    {
                        string unitAttackableAbsentLog = stepCompleteCondition.MobAttackableSkipIfAbsent ?
                            $"[Condition PASS] Unit to check if attackable is dead or absent"
                            : $"[Condition FAIL] Unit to check if attackable is dead or absent";
                        Logger.LogOnce(unitAttackableAbsentLog);
                        return stepCompleteCondition.MobAttackableSkipIfAbsent;
                    }
                    bool conditionMet = stepCompleteCondition.MobAttackableMustReturnTrue ? unitAttackable.IsAttackable : !unitAttackable.IsAttackable;
                    string unitAttackableLog = unitAttackable.IsAttackable ? "Unit is attackable" : "Unit is not attackable";
                    string unitAttackableFinalLog = conditionMet ? $"[Condition PASS] {unitAttackableLog}" : $"[Condition FAIL] {unitAttackableLog}";
                    Logger.LogOnce(unitAttackableFinalLog);
                    return conditionMet;

                case CompleteConditionType.FlagsChanged:
                    WoWGameObject objectFlagsChanged = ObjectManager.GetWoWGameObjectByEntry(stepCompleteCondition.FlagsChangedGameObjectId)
                        .OrderBy(wObject => wObject.GetDistance)
                        .FirstOrDefault();
                    if (objectFlagsChanged == null)
                    {
                        Logger.LogOnce($"WARNING, object to check for flag change with ID [{stepCompleteCondition.FlagsChangedGameObjectId}] couldn't be found", true);
                        return false;
                    }
                    bool resultFlagsChanged = objectFlagsChanged.FlagsInt != stepCompleteCondition.InitialFlags;
                    if (resultFlagsChanged) Logger.LogOnce($"[Condition PASS] {objectFlagsChanged.Name}'s flags have changed from {stepCompleteCondition.InitialFlags} to {objectFlagsChanged.FlagsInt}");
                    else Logger.LogOnce($"[Condition FAIL] {objectFlagsChanged.Name}'s flags haven't changed ({objectFlagsChanged.FlagsInt})");
                    return resultFlagsChanged;

                case CompleteConditionType.HaveItem:
                    bool resultHaveItem = ItemsManager.GetItemCountByIdLUA((uint)stepCompleteCondition.HaveItemId) > 0;
                    bool haveItemFinalResult = stepCompleteCondition.HaveItemMustReturnTrue ? resultHaveItem : !resultHaveItem;
                    string haveItemTxt = resultHaveItem ? $"You have item {stepCompleteCondition.HaveItemId} in your bags"
                        : $"You don't have item {stepCompleteCondition.HaveItemId} in your bags";
                    string haveItemPassTxt = haveItemFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{haveItemPassTxt} {haveItemTxt}");
                    return haveItemFinalResult;

                case CompleteConditionType.MobDead:
                    WoWUnit deadmob = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == stepCompleteCondition.DeadMobId)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    bool resultMobDead = deadmob == null || deadmob.IsDead;
                    bool mobDeadFinalResult = stepCompleteCondition.MobDeadMustReturnTrue ? resultMobDead : !resultMobDead;
                    string mobDeadTxt = resultMobDead ? $"Unit {stepCompleteCondition.DeadMobId} is dead or absent"
                        : $"Unit {stepCompleteCondition.DeadMobId} is not dead or absent";
                    string mobDeadPassTxt = mobDeadFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{mobDeadPassTxt} {mobDeadTxt}");
                    return mobDeadFinalResult;

                case CompleteConditionType.MobAtPosition:
                    WoWUnit unitToCheck = ObjectManager
                        .GetWoWUnitByEntry(stepCompleteCondition.MobAtPositionId)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    Vector3 mobvec = stepCompleteCondition.MobAtPositionVector;
                    bool resultMobAtPosition = unitToCheck != null && mobvec.DistanceTo(unitToCheck.Position) < 5;
                    bool mobAtPositionFinalResult = stepCompleteCondition.MobAtPositionMustReturnTrue ? resultMobAtPosition : !resultMobAtPosition;
                    string mobAtPositionTxt = resultMobAtPosition ? $"Unit {unitToCheck.Name} is at the expected position"
                        : $"Unit {stepCompleteCondition.MobAtPositionId} is not at the expect position or is absent";
                    string mobAtPositionPassTxt = mobAtPositionFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{mobAtPositionPassTxt} {mobAtPositionTxt}");
                    return mobAtPositionFinalResult;

                case CompleteConditionType.CanGossip:
                    WoWUnit gossipUnit = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == stepCompleteCondition.CanGossipMobId)
                        .FirstOrDefault();
                    bool resultGossipUnit = gossipUnit != null && !gossipUnit.UnitNPCFlags.HasFlag(UnitNPCFlags.Gossip);
                    bool unitCanGossipFinalResult = stepCompleteCondition.CanGossipMustReturnTrue ? resultGossipUnit : !resultGossipUnit;
                    string unitCanGossipTxt = resultGossipUnit ? $"Unit {stepCompleteCondition.CanGossipMobId} can gossip"
                        : $"Unit {stepCompleteCondition.CanGossipMobId} can't gossip or is absent";
                    string unitCanGossipPassTxt = unitCanGossipFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{unitCanGossipPassTxt} {unitCanGossipTxt}");
                    return unitCanGossipFinalResult;

                case CompleteConditionType.LOSCheck:
                    if (stepCompleteCondition.LOSPositionVectorFrom == null || stepCompleteCondition.LOSPositionVectorTo == null)
                    {
                        Logger.LogError($"[Condition FAIL] LoS check is missing a vector!");
                        return false;
                    }
                    TraceLine.ClearCache();
                    bool losResult = !TraceLine.TraceLineGo(stepCompleteCondition.LOSPositionVectorFrom, stepCompleteCondition.LOSPositionVectorTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                    bool losFinalResult = stepCompleteCondition.LOSMustReturnTrue ? losResult : !losResult;
                    string losTxt = losResult ? "LoS result is positive" : "LoS result is negative";
                    string losPassTxt = losFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{losPassTxt} {losTxt}");
                    return losFinalResult;

                default:
                    return true;
            }
        }

        public void MarkAsCompleted()
        {
            if (!IsCompleted)
            {
                Logger.Log($"Marked {Name} as completed");
                IsCompleted = true;
            }
        }
    }
}
