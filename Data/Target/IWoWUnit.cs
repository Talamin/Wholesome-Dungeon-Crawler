using robotManager.Helpful;
using System.Collections.Generic;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    public interface IWoWUnit
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
        Reaction Reaction { get; }
        UnitFlags UnitFlags { get; }
        bool IsAttackingGroup { get; }
        bool IsAttackingMe { get; }
        bool IsPartyMember { get; }
        WoWClass WoWClass { get; }     
        uint GetBaseAdress { get; }
        bool Fleeing { get; }
        IReadOnlyDictionary<uint, IAura> Auras { get; }

        bool HasDrinkBuff { get; }
        bool HasFoodBuff { get; }
        WoWUnit WowUnit { get; }
    }
}
