using robotManager.Helpful;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IWoWLocalPlayer : IWoWPlayer
    {
        public Vector3 PositionCorpse { get; }
    }
}
