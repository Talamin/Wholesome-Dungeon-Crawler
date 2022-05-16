namespace WholesomeDungeonCrawler.Data
{
    public interface IEntityCache : ICycleable
    {
        IWoWUnit Target { get; }
        IWoWUnit Pet { get; }
        IWoWUnit Me { get; }

        IWoWUnit[] EnemyUnitsNearTarget { get; }
        IWoWUnit[] EnemyUnitsNearPlayer { get; }
        IWoWUnit[] InterruptibleEnemyUnits { get; }
        IWoWUnit[] EnemyUnitsTargetingPlayer { get; }
        IWoWUnit[] EnemyUnitsTargetingGroup { get; }
        IWoWUnit[] EnemyUnitsLootable { get; }
        IWoWUnit[] EnemyAttackingGroup { get; }
        IWoWUnit[] EnemyUnitsList { get; }
        IWoWUnit[] ListGroupMember { get; }

    }
}
