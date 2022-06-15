using robotManager.Helpful;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    internal sealed class CachedWoWLocalPlayer : CachedWoWPlayer, IWoWLocalPlayer
    {
        public Vector3 PositionCorpse { get; }
        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {
            PositionCorpse = player.PositionCorpse;
        }
    }

}
