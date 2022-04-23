using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class CachedWoWPlayer : CachedWoWUnit, IWoWPlayer
    {
        public CachedWoWPlayer(WoWPlayer player) : base(player)
        {
        }
    }
}
