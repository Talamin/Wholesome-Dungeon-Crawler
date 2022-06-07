namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IWoWPlayer : IWoWUnit
    {
        public bool IsConnected { get; }
    }
}
