using robotManager.Helpful;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class RegroupStep : Step
    {
        private RegroupModel _regroupModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }
        public override int Order { get; }
        public Vector3 RegroupSpot { get; private set; }

        public RegroupStep(RegroupModel regroupModel, IEntityCache entityCache)
        {
            _regroupModel = regroupModel;
            _entityCache = entityCache;
            Name = regroupModel.Name;
            Order = regroupModel.Order;
            RegroupSpot = regroupModel.RegroupSpot;
        }

        public override void Run()
        {
            // Move to regroup spot location
            if (!MovementManager.InMovement 
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 5f)
            {
                Logger.Log($"[{_regroupModel.Name}] Moving to regroup spot location");
                GoToTask.ToPosition(RegroupSpot, 0.5f);
                IsCompleted = false;
                return;
            }

            // Move to the exact regroup spot
            if (!MovementManager.InMovement
                && !MovementManager.InMoveTo
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 1f)
            {
                Logger.Log($"[{_regroupModel.Name}] Moving precisely to regroup spot");
                MovementManager.MoveTo(RegroupSpot);
                IsCompleted = false;
                return;
            }

            // Check if everyone is here
            if (_entityCache.ListGroupMember.Length == _entityCache.ListPartyMemberNames.Count
                && _entityCache.ListGroupMember.All(member => member.PositionWithoutType.DistanceTo(RegroupSpot) <= 5f)
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) <= 5f)
            {
                if (_entityCache.IAmTank)
                {
                    Thread.Sleep(3000);
                }
                else
                {
                    Thread.Sleep(4000);
                }

                if (_entityCache.ListGroupMember.All(member => !member.HasDrinkBuff && !member.HasFoodBuff))
                {
                    Logger.Log($"The team has regrouped. Proceeding.");
                    IsCompleted = true;
                    return;
                }
            }

            IsCompleted = false;
            Thread.Sleep(500);
        }
    }
}
