using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public class ExecuteStep : Step
    {
        private ExecuteModel _executeModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }

        public ExecuteStep(ExecuteModel executeModel, IEntityCache entityCache)
        {
            _executeModel = executeModel;
            _entityCache = entityCache;
            Name = executeModel.Name;
        }

        public override void Run()
        {


        }
    }
}
