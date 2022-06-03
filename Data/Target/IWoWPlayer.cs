namespace WholesomeDungeonCrawler.Data
{
    public interface IWoWPlayer : IWoWUnit
    {
        public bool IsConnected { get; }
    }
}
