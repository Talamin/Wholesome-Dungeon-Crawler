using robotManager.Helpful;
using System;
using System.Collections.Generic;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data.Model
{
    public abstract class StepModel
    {
        public string Name { get; set; }
        public int Order { get; set; }        
        public StepCompleteConditionModel CompleteCondition { get; set; }
    }

    public class StepCompleteConditionModel
    {
        public bool HasCompleteCondition { get; set; }
        public CompleteConditionType ConditionType { get; set; }
        public int GameObjectId { get; set; }
        public int InitialFlags { get; set; }
        public int ItemId { get; set; }
        public string CSharpCondition { get; set; }
        public int DeadMobId { get; set; }
        public int MobPositionId { get; set; }
        public Vector3 MobPositionVector { get; set; }
        public Vector3 LOSPositionVector { get; set; }
    }


    public class ExecuteModel : StepModel
    {
        public string Action { get; set; }
        public bool CheckCompletion { get; set; }
    }

    public class GoToModel : StepModel
    {
        public float Precision { get; set; }
        public Vector3 TargetPosition { get; set; }
    }

    public class MoveAlongPathModel : StepModel
    {
        public List<Vector3> Path { get; set; }
    }

    public class MoveToUnitModel : StepModel
    {
        public Vector3 ExpectedPosition { get; set; }
        public bool FindClosest { get; set; }
        public bool SkipIfNotFound { get; set; }
        public int UnitId { get; set; }
        public bool Interactwithunit { get; set; }
        public int Gossip { get; set; }
    }

    public class PickupObjectModel : StepModel
    {
        public int ObjectId { get; set; }
        public uint ItemId { get; set; }
        public Vector3 ExpectedPosition { get; set; }
        public bool FindClosest { get; set; }
        public bool StrictPosition { get; set; }
        public float InteractDistance { get; set; }
    }

    public class InteractWithModel : StepModel
    {
        public int ObjectId { get; set; }
        public Vector3 ExpectedPosition { get; set; }

        public bool StrictPosition { get; set; }
        public bool FindClosest { get; set; }

        public Func<WoWGameObject, bool> isCompleted { get; set; }
    }

    public class DefendSpotModel : StepModel
    {
        public Vector3 DefendPosition { get; set; }
        public int Timer { get; set; }
        public float Precision { get; set; }
    }

    public class FollowUnitModel : StepModel
    {
        public Vector3 ExpectedStartPosition { get; set; }
        public Vector3 ExpectedEndPosition { get; set; }
        public bool FindClosest { get; set; }
        public bool SkipIfNotFound { get; set; }
        public int UnitId { get; set; }
    }
}
