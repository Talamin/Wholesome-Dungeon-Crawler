using System.Collections.Generic;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IEntityCache : ICycleable
    {
        IWoWUnit Target { get; }
        IWoWUnit Pet { get; }
        IWoWLocalPlayer Me { get; }
        IWoWPlayer TankUnit { get; }
        IWoWUnit[] EnemiesAttackingGroup { get; }
        IWoWUnit[] EnemyUnitsList { get; }
        IWoWPlayer[] ListGroupMember { get; }
        List<string> ListPartyMemberNames { get; }
        List<IWoWUnit> NpcsToDefend { get; }
        List<IWoWUnit> LootableUnits { get; }
        IWoWUnit[] GroupPets { get; }
        bool IAmTank { get; }

        void AddNpcIdToDefend(int npcId);
        void ClearNpcListIdToDefend();
        void CacheGroupMembers(string trigger);
    }
}
