using robotManager.Helpful;
using WholesomeDungeonCrawler.Data;

namespace WholesomeDungeonCrawler.Manager
{
    internal interface IPartyChatManager : ICycleable
    {
        Vector3 TankPosition { get; }
    }
}
