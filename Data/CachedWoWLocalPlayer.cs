using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal sealed class CachedWoWLocalPlayer : CachedWoWPlayer, IWoWLocalPlayer
    {

        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {

        }
    }

}
