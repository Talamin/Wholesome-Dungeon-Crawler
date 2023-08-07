using System.Collections.Generic;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.Profiles;

namespace WholesomeDungeonCrawler.Managers
{
    interface IProfileManager : ICycleable
    {
        List<DungeonModel> AvailableDungeons { get; }
        public IProfile CurrentDungeonProfile { get; }
        bool ProfileIsRunning { get; }
        void LoadProfile(bool safeWait);
        void UnloadCurrentProfile();
        void UpdateAvailableDungeonList();
    }
}
