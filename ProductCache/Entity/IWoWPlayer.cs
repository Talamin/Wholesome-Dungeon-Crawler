using wManager.Wow.Enums;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IWoWPlayer : IWoWUnit
    {
        public bool IsConnected { get; }
        public WoWClass WoWClass { get; }
    }
}
