using robotManager.Helpful;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    interface IWoWUnit
    {
        string Name { get; }
        ulong Guid { get; }
        ulong TargetGuid { get; }
        bool Valid { get; }
        bool Dead { get; }
        bool IsLootable { get; }
        Vector3 PositionWithoutType { get; }
        double HealthPercent { get; }
        double ManaPercent { get; }
        double RagePercent { get; }
        double FocusPercent { get; }
        bool InCombatFlagOnly { get; }
        UnitFlags UnitFlags { get; }
        bool IsAttackingGroup { get; }
        bool IsPartyMember { get; }
        public WoWClass WoWClass { get; }

        public uint GetBaseAdress { get; }
    }
}
