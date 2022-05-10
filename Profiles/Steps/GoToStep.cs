using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class GoToStep : Step
    {
        private GoToModel _gotoModel;

        public GoToStep(GoToModel goToModel)
        {
            _gotoModel = goToModel;
        }

        public override void Run()
        {

            if (ObjectManager.Me.PositionWithoutType.DistanceTo(_gotoModel.TargetPosition) < _gotoModel.Precision)
            {
                IsCompleted = true;
                return;
            }

            GoToTask.ToPosition(_gotoModel.TargetPosition);
            IsCompleted = false;
        }
    }
}