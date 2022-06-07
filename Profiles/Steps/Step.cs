using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
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
            switch (stepCompleteCondition.ConditionType)
            {
                case CompleteConditionType.Csharp:
                    return true;
                case CompleteConditionType.FlagsChanged:
                    var obj = ObjectManager.GetWoWGameObjectByEntry(stepCompleteCondition.GameObjectId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    return obj.FlagsInt != stepCompleteCondition.InitialFlags;
                case CompleteConditionType.HaveItem:
                    var bagItems = ItemsManager.GetItemCountByIdLUA((uint)stepCompleteCondition.ItemId);
                    return bagItems > 0;
                case CompleteConditionType.MobDead:
                    var deadmob = ObjectManager.GetObjectWoWUnit().Where(x => x.Entry == stepCompleteCondition.DeadMobId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    return (deadmob != null && deadmob.IsDead) || deadmob == null;
                case CompleteConditionType.MobPosition:
                    var mobpos = ObjectManager.GetWoWUnitByEntry(stepCompleteCondition.MobPositionId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    var mobvec = stepCompleteCondition.MobPositionVector;
                    return mobpos != null && mobvec.DistanceTo(mobpos.Position) < 5;
                case CompleteConditionType.CanGossip:
                    var unit = ObjectManager.GetObjectWoWUnit().Where(x => x.Entry == stepCompleteCondition.MobId).FirstOrDefault();
                    return !unit.UnitNPCFlags.HasFlag(wManager.Wow.Enums.UnitNPCFlags.Gossip);
                case CompleteConditionType.LOSCheck:
                    return !TraceLine.TraceLineGo(stepCompleteCondition.LOSPositionVector);
                default:
                    return false;
            }
        }

        public void MarkAsCompleted() => IsCompleted = true;
    }
}
