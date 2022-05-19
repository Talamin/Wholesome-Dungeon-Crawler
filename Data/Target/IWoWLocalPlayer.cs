using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Data
{
    public interface IWoWLocalPlayer : IWoWPlayer
    {
        public Vector3 PositionCorpse { get; }
    }
}
