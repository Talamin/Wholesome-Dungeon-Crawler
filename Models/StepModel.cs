using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Models
{
    public abstract class StepModel
    {
        public string Name { get; set; }
        public StepCompleteConditionModel CompleteCondition { get; set; }
        public FactionType StepFaction { get; set; } = FactionType.Neutral;
        public LFGRoles StepRole { get; set; } = LFGRoles.Unspecified;
        public bool HasCompleteCondition => CompleteCondition != null && CompleteCondition.ConditionType != CompleteConditionType.None;
    }

    public class StepCompleteConditionModel
    {
        public CompleteConditionType ConditionType { get; set; }

        // Timer
        public Timer ConditionTimer { get; set; }
        public double GetTimerTimeLeft => ConditionTimer == null || ConditionTimer.IsReady ? 0 : ConditionTimer.TimeLeft();
        public int TimerTimeInSeconds { get; set; }

        // Have Item
        public int HaveItemId { get; set; }
        public bool HaveItemMustReturnTrue { get; set; } = true;

        // Can Gossip
        public int CanGossipMobId { get; set; }
        public bool CanGossipMustReturnTrue { get; set; } = true;

        // Flags changed
        public int InitialFlags { get; set; }
        public int FlagsChangedGameObjectId { get; set; }

        // Mob dead or absent
        public int DeadMobId { get; set; }
        public bool MobDeadMustReturnTrue { get; set; } = true;

        // Mob at position
        public int MobAtPositionId { get; set; }
        public Vector3 MobAtPositionVector { get; set; }
        public bool MobAtPositionMustReturnTrue { get; set; } = true;

        // LoS
        public Vector3 LOSPositionVectorFrom { get; set; }
        public Vector3 LOSPositionVectorTo { get; set; }
        public bool LOSMustReturnTrue { get; set; } = true;

        // Unit is attackable
        public int MobAttackableEntry { get; set; }
        public bool MobAttackableMustReturnTrue { get; set; }
        public bool MobAttackableSkipIfAbsent { get; set; }
    }

    public class MoveAlongPathModel : StepModel
    {
        public List<Vector3> Path { get; set; }
        public bool IgnoreFightsDuringPath { get; set; }
    }

    public class TalkToUnitModel : StepModel
    {
        public Vector3 ExpectedPosition { get; set; }
        public bool SkipIfNotFound { get; set; }
        public int UnitId { get; set; }
        public int GossipIndex { get; set; }
    }

    public class InteractWithModel : StepModel
    {
        public string ObjectId { get; set; }
        public Vector3 ExpectedPosition { get; set; }
        public int InteractDistance { get; set; }
        public bool SkipIfNotFound { get; set; }
    }

    public class DefendSpotModel : StepModel
    {
        public Vector3 DefendPosition { get; set; }
        public int Timer { get; set; }
        public int DefendSpotRadius { get; set; } = 30;
        public int DefendSpotZLimit { get; set; } = 5;
        public bool ResetTimerOnCombat { get; set; }
    }
    public class DefendVioletHoldModel : StepModel
    {
        public Vector3 DefendPosition { get; set; }
        public int Timer { get; set; }
        public int DefendSpotRadius { get; set; } = 30;
        public int DefendSpotZLimit { get; set; } = 5;
        public bool ResetTimerOnCombat { get; set; }
    }

    public class FollowUnitModel : StepModel
    {
        public Vector3 ExpectedStartPosition { get; set; }
        public Vector3 ExpectedEndPosition { get; set; }
        public bool SkipIfNotFound { get; set; }
        public int UnitId { get; set; }
    }

    public class RegroupModel : StepModel
    {
        public Vector3 RegroupSpot { get; set; }
    }

    public class LeaveDungeonModel : StepModel
    {
    }

    public class JumpToStepModel : StepModel
    {
        public string StepToJumpTo { get; set; }
    }

    public class PullToSafeSpotModel : StepModel
    {
        public Vector3 SafeSpotPosition { get; set; }
        public int SafeSpotRadius { get; set; } = 10;
        public Vector3 ZoneToClearPosition { get; set; }
        public int ZoneToClearRadius { get; set; } = 30;
        public int ZoneToClearZLimit { get; set; } = 5;
    }
}
