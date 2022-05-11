using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Data;
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
        private readonly IEntityCache _entityCache;

        public GoToStep(GoToModel goToModel, IEntityCache entityCache)
        {
            _gotoModel = goToModel;
            _entityCache = entityCache;
        }

        public override void Run()
        {

            if (_entityCache.Me.PositionWithoutType.DistanceTo(_gotoModel.TargetPosition) < _gotoModel.Precision)
            {
                IsCompleted = true;
                return;
            }

            GoToTask.ToPosition(_gotoModel.TargetPosition);
            IsCompleted = false;
        }
    }
}