using robotManager.Helpful;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    internal sealed class CachedWoWLocalPlayer : CachedWoWPlayer, ICachedWoWLocalPlayer
    {
        public Vector3 PositionCorpse { get; }
        public bool Swimming { get; }
        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {
            PositionCorpse = player.PositionCorpse;
            Swimming = player.IsSwimming;
        }
    }

}
