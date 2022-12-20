using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.Profiles.Steps;
using static WholesomeDungeonCrawler.Managers.PartyChatManager;

namespace WholesomeDungeonCrawler.Managers
{
    internal interface IPartyChatManager : ICycleable
    {
        PlayerStatus TankStatus { get; }

        void Broadcast(ChatMessageType messageType, string message);
        void SetRegroupStep(RegroupStep regroupStep);
        void PartyReadyReceived();
    }
}
