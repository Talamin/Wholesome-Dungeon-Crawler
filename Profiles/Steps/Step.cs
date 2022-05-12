using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public abstract class Step : IStep
    {
        public bool IsCompleted { get; protected set; }

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
                    var bagItems = wManager.Wow.Helpers.ItemsManager.GetItemCountByIdLUA((uint)stepCompleteCondition.ItemId);
                    return bagItems > 0;
                case CompleteConditionType.MobDead:
                    var deadmob = ObjectManager.GetObjectWoWUnit().Where(x => x.Entry == stepCompleteCondition.DeadMobId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    return (deadmob != null && deadmob.IsDead) || deadmob == null;
                case CompleteConditionType.MobPosition:
                    var mobpos = ObjectManager.GetWoWUnitByEntry(stepCompleteCondition.MobPositionId).OrderBy(x => x.GetDistance).FirstOrDefault();
                    var mobvec = stepCompleteCondition.MobPositionVector;
                    return mobpos != null && mobvec.DistanceTo(mobpos.Position) < 5;
                default:
                    return false;
            }
        }
    }
}
