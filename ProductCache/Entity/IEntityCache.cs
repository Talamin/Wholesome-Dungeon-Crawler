using System.Collections.Generic;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public delegate void TankOMHandler();

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
        IWoWUnit[] GroupPets { get; }

        bool IAmTank { get; }

        event TankOMHandler OnTankEnteringOM;

        void AddNpcIdToDefend(int npcId);
        void ClearNpcListIdToDefend();
    }
}
