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
    public class ExecuteStep : Step
    {
        private ExecuteModel _executeModel;
        private readonly IEntityCache _entityCache;

        public ExecuteStep(ExecuteModel executeModel, IEntityCache entityCache)
        {
            _executeModel = executeModel;
            _entityCache = entityCache;
        }

        public override void Run()
        {

            
        }
    }
}
