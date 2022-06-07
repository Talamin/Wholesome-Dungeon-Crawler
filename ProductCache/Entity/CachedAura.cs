using wManager.Wow.Class;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    internal sealed class CachedAura : IAura
    {
        public int Stacks { get; }
        public int TimeLeft { get; }

        public CachedAura(Aura aura)
        {
            Stacks = aura.Stack;
            TimeLeft = aura.TimeLeft;
        }
    }

}
