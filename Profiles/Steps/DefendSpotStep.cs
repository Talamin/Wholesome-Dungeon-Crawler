using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using robotManager.Helpful;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Data.Model;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class DefendSpotStep : Step
    {
        private DefendSpotModel _defendSpotModel;
        private readonly IEntityCache _entityCache;

        public DefendSpotStep(DefendSpotModel defendSpotModel, IEntityCache entityCache)
        {
            _defendSpotModel = defendSpotModel;
            _entityCache = entityCache;
        }

        private Timer stepTimer = new Timer();

        public override void Run()
        {
            if (_entityCache.Me.PositionWithoutType.DistanceTo(_defendSpotModel.DefendPosition) < _defendSpotModel.Precision && stepTimer.IsReady)
            {
                if (!_defendSpotModel.CompleteCondition.HasCompleteCondition)
                {
                    IsCompleted = true;
                    return;
                }
                else if (EvaluateCompleteCondition(_defendSpotModel.CompleteCondition))
                {
                    IsCompleted = true;
                    return;
                }
            }

            if (!MovementManager.InMovement || MovementManager.CurrentMoveTo.DistanceTo(_defendSpotModel.DefendPosition) > _defendSpotModel.Precision)
            {
                GoToTask.ToPosition(_defendSpotModel.DefendPosition);
            }

            IsCompleted = false;

        }
    }
}
