using wManager.Wow.Enums;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface ICachedWoWPlayer : ICachedWoWUnit
    {
        public bool IsConnected { get; }
        public WoWClass WoWClass { get; }
    }
}
