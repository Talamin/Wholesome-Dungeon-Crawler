using System.Collections.Generic;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IEntityCache : ICycleable
    {
        ICachedWoWUnit Target { get; }
        ICachedWoWUnit Pet { get; }
        ICachedWoWLocalPlayer Me { get; }
        ICachedWoWPlayer TankUnit { get; }
        ICachedWoWUnit[] EnemiesAttackingGroup { get; }
        ICachedWoWUnit[] EnemyUnitsList { get; }
        ICachedWoWUnit[] InterestingUnitsList { get; }
        ICachedWoWPlayer[] ListGroupMember { get; }
        List<string> ListPartyMemberNames { get; }
        List<ICachedWoWUnit> NpcsToDefend { get; }
        List<ICachedWoWUnit> LootableUnits { get; }
        //ICachedWoWUnit[] GroupPets { get; }
        bool IAmTank { get; }

        void AddNpcIdToDefend(int npcId);
        void ClearNpcListIdToDefend();
        void CacheGroupMembers(string trigger);
    }
}
