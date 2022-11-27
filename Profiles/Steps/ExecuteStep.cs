using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class ExecuteStep : Step
    {
        private ExecuteModel _executeModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }
        public override int Order { get; }

        public ExecuteStep(ExecuteModel executeModel, IEntityCache entityCache)
        {
            _executeModel = executeModel;
            _entityCache = entityCache;
            Name = executeModel.Name;
            Order = executeModel.Order;
        }

        public override void Run()
        {
        }
    }
}
