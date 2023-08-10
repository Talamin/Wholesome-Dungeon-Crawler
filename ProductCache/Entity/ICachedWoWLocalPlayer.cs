using robotManager.Helpful;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface ICachedWoWLocalPlayer : ICachedWoWPlayer
    {
        public Vector3 PositionCorpse { get; }
        public bool Swimming { get; }
    }
}
