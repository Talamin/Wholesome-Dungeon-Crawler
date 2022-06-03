using wManager.Wow;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class CachedWoWPlayer : CachedWoWUnit, IWoWPlayer
    {
        public bool IsConnected { get; }
        public CachedWoWPlayer(WoWPlayer player) : base(player)
        {
            IsConnected = player.IsValid && Memory.WowMemory.Memory.ReadBoolean(player.GetBaseAddress + 8);
        }
    }
}
