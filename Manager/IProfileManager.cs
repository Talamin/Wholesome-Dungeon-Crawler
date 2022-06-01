using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Profiles;

namespace WholesomeDungeonCrawler.Manager
{
    interface IProfileManager : ICycleable
    {

        public IProfile CurrentDungeonProfile { get; }
    }
}
