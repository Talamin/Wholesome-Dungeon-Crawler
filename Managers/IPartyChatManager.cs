using robotManager.Helpful;
using WholesomeDungeonCrawler.ProductCache;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IPartyChatManager : ICycleable
    {
        Vector3 TankPosition { get; }
    }
}
