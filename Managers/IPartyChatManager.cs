using WholesomeDungeonCrawler.ProductCache;
using static WholesomeDungeonCrawler.Managers.PartyChatManager;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IPartyChatManager : ICycleable
    {
        PlayerStatus TankStatus { get; }

        void Broadcast(ChatMessageType messageType, string message);
    }
}
