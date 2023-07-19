using robotManager.Helpful;
using System.Collections.Generic;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    public interface IWoWUnit
    {
        string Name { get; }
        int Entry { get; }
        ulong Guid { get; }
        ulong TargetGuid { get; }
        bool IsValid { get; }
        bool IsDead { get; }
        bool IsLootable { get; }
        Vector3 PositionWithoutType { get; }
        double HealthPercent { get; }
        long Health { get; }
        long Mana { get; }
        double ManaPercent { get; }
        double RagePercent { get; }
        double FocusPercent { get; }
        bool InCombatFlagOnly { get; }
        Reaction Reaction { get; }
        UnitFlags UnitFlags { get; }
        bool IsAttackingGroup { get; }
        bool IsAttackingMe { get; }
        bool IsPartyMember { get; }
        uint GetBaseAddress { get; }
        bool Fleeing { get; }
        IReadOnlyDictionary<uint, IAura> Auras { get; }
        bool HasDrinkBuff { get; }
        bool HasFoodBuff { get; }
        WoWUnit WowUnit { get; }
        WoWUnit Target { get; }
}
}
