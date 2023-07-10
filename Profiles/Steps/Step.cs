using robotManager.Helpful;
using System;
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
        public StepCompleteConditionModel StepCompleteConditionModel { get; private set; }

        public Step(StepCompleteConditionModel stepCompleteCondition)
        {
            StepCompleteConditionModel = stepCompleteCondition;
        }

        public abstract void Run();

        public bool EvaluateCompleteCondition()
        {
            if (StepCompleteConditionModel == null)
            {
                // No step condition
                return true;
            }

            switch (StepCompleteConditionModel.ConditionType)
            {
                case CompleteConditionType.Timer:
                    if (StepCompleteConditionModel.ConditionTimer == null)
                    {
                        StepCompleteConditionModel.ConditionTimer = new Timer(StepCompleteConditionModel.TimerTimeInSeconds * 1000);
                        return false;
                    }

                    if (StepCompleteConditionModel.ConditionTimer.IsReady)
                    {
                        Logger.Log($"[Condition PASS] Timer is over.");
                        return true;
                    }
                    else
                    {
                        if ((StepCompleteConditionModel.ConditionTimer.TimeLeft() / 1000) % 5 == 0) 
                            Logger.LogOnce($"[Condition FAIL] Time left: {StepCompleteConditionModel.ConditionTimer.TimeLeft() / 1000} seconds");
                        return false;
                    }

                case CompleteConditionType.MobAttackable:
                    WoWUnit unitAttackable = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == StepCompleteConditionModel.MobAttackableEntry)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    // Unit missing or dead
                    if (unitAttackable == null || unitAttackable.IsDead)
                    {
                        string unitAttackableAbsentLog = StepCompleteConditionModel.MobAttackableSkipIfAbsent ?
                            $"[Condition PASS] Unit to check if attackable is dead or absent"
                            : $"[Condition FAIL] Unit to check if attackable is dead or absent";
                        Logger.LogOnce(unitAttackableAbsentLog);
                        return StepCompleteConditionModel.MobAttackableSkipIfAbsent;
                    }
                    bool conditionMet = StepCompleteConditionModel.MobAttackableMustReturnTrue ? unitAttackable.IsAttackable : !unitAttackable.IsAttackable;
                    string unitAttackableLog = unitAttackable.IsAttackable ? "Unit is attackable" : "Unit is not attackable";
                    string unitAttackableFinalLog = conditionMet ? $"[Condition PASS] {unitAttackableLog}" : $"[Condition FAIL] {unitAttackableLog}";
                    Logger.LogOnce(unitAttackableFinalLog);
                    return conditionMet;

                case CompleteConditionType.FlagsChanged:
                    WoWGameObject objectFlagsChanged = ObjectManager.GetWoWGameObjectByEntry(StepCompleteConditionModel.FlagsChangedGameObjectId)
                        .OrderBy(wObject => wObject.GetDistance)
                        .FirstOrDefault();
                    if (objectFlagsChanged == null)
                    {
                        Logger.LogOnce($"WARNING, object to check for flag change with ID [{StepCompleteConditionModel.FlagsChangedGameObjectId}] couldn't be found", true);
                        return false;
                    }
                    bool resultFlagsChanged = objectFlagsChanged.FlagsInt != StepCompleteConditionModel.InitialFlags;
                    if (resultFlagsChanged) Logger.LogOnce($"[Condition PASS] {objectFlagsChanged.Name}'s flags have changed from {StepCompleteConditionModel.InitialFlags} to {objectFlagsChanged.FlagsInt}");
                    else Logger.LogOnce($"[Condition FAIL] {objectFlagsChanged.Name}'s flags haven't changed ({objectFlagsChanged.FlagsInt})");
                    return resultFlagsChanged;

                case CompleteConditionType.HaveItem:
                    bool resultHaveItem = ItemsManager.GetItemCountByIdLUA((uint)StepCompleteConditionModel.HaveItemId) > 0;
                    bool haveItemFinalResult = StepCompleteConditionModel.HaveItemMustReturnTrue ? resultHaveItem : !resultHaveItem;
                    string haveItemTxt = resultHaveItem ? $"You have item {StepCompleteConditionModel.HaveItemId} in your bags"
                        : $"You don't have item {StepCompleteConditionModel.HaveItemId} in your bags";
                    string haveItemPassTxt = haveItemFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{haveItemPassTxt} {haveItemTxt}");
                    return haveItemFinalResult;

                case CompleteConditionType.MobDead:
                    WoWUnit deadmob = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == StepCompleteConditionModel.DeadMobId)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    bool resultMobDead = deadmob == null || deadmob.IsDead;
                    bool mobDeadFinalResult = StepCompleteConditionModel.MobDeadMustReturnTrue ? resultMobDead : !resultMobDead;
                    string mobDeadTxt = resultMobDead ? $"Unit {StepCompleteConditionModel.DeadMobId} is dead or absent"
                        : $"Unit {StepCompleteConditionModel.DeadMobId} is not dead or absent";
                    string mobDeadPassTxt = mobDeadFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{mobDeadPassTxt} {mobDeadTxt}");
                    return mobDeadFinalResult;

                case CompleteConditionType.MobAtPosition:
                    WoWUnit unitToCheck = ObjectManager
                        .GetWoWUnitByEntry(StepCompleteConditionModel.MobAtPositionId)
                        .OrderBy(unit => unit.GetDistance)
                        .FirstOrDefault();
                    Vector3 mobvec = StepCompleteConditionModel.MobAtPositionVector;
                    bool resultMobAtPosition = unitToCheck != null && mobvec.DistanceTo(unitToCheck.Position) < 5;
                    bool mobAtPositionFinalResult = StepCompleteConditionModel.MobAtPositionMustReturnTrue ? resultMobAtPosition : !resultMobAtPosition;
                    string mobAtPositionTxt = resultMobAtPosition ? $"Unit {unitToCheck.Name} is at the expected position"
                        : $"Unit {StepCompleteConditionModel.MobAtPositionId} is not at the expect position or is absent";
                    string mobAtPositionPassTxt = mobAtPositionFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{mobAtPositionPassTxt} {mobAtPositionTxt}");
                    return mobAtPositionFinalResult;

                case CompleteConditionType.CanGossip:
                    WoWUnit gossipUnit = ObjectManager.GetObjectWoWUnit()
                        .Where(unit => unit.Entry == StepCompleteConditionModel.CanGossipMobId)
                        .FirstOrDefault();
                    bool resultGossipUnit = gossipUnit != null && !gossipUnit.UnitNPCFlags.HasFlag(UnitNPCFlags.Gossip);
                    bool unitCanGossipFinalResult = StepCompleteConditionModel.CanGossipMustReturnTrue ? resultGossipUnit : !resultGossipUnit;
                    string unitCanGossipTxt = resultGossipUnit ? $"Unit {StepCompleteConditionModel.CanGossipMobId} can gossip"
                        : $"Unit {StepCompleteConditionModel.CanGossipMobId} can't gossip or is absent";
                    string unitCanGossipPassTxt = unitCanGossipFinalResult ? "[Condition PASS]" : "[Condition FAIL]";
                    Logger.LogOnce($"{unitCanGossipPassTxt} {unitCanGossipTxt}");
                    return unitCanGossipFinalResult;

                case CompleteConditionType.LOSCheck:
                    if (StepCompleteConditionModel.LOSPositionVectorFrom == null || StepCompleteConditionModel.LOSPositionVectorTo == null)
                    {
                        Logger.LogError($"[Condition FAIL] LoS check is missing a vector!");
                        return false;
                    }
                    TraceLine.ClearCache();
                    bool losResult = !TraceLine.TraceLineGo(StepCompleteConditionModel.LOSPositionVectorFrom, StepCompleteConditionModel.LOSPositionVectorTo, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS);
                    bool losFinalResult = StepCompleteConditionModel.LOSMustReturnTrue ? losResult : !losResult;
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
