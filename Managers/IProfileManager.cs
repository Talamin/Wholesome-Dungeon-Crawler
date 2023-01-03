using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.Profiles;

namespace WholesomeDungeonCrawler.Managers
{
    interface IProfileManager : ICycleable
    {
        public IProfile CurrentDungeonProfile { get; }
        bool ProfileIsRunning { get; }
        void LoadProfile(bool safeWait);
        void UnloadCurrentProfile();
    }
}
