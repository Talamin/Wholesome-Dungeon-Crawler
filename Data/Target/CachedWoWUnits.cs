using robotManager.Helpful;
using System.Collections.Generic;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class CachedWoWUnit : IWoWUnit
    {
        public string Name { get; }
        public ulong Guid { get; }
        public ulong TargetGuid { get; }
        public bool Valid { get; }
        public bool Dead { get; }
        public Vector3 PositionWithoutType { get; }
        public double HealthPercent { get; }
        public double ManaPercent { get; }
        public double RagePercent { get; }
        public double FocusPercent { get; }
        public bool InCombatFlagOnly { get; }
        public UnitFlags UnitFlags { get; }
        public IReadOnlyDictionary<uint, IAura> Auras { get; }
        public bool IsLootable { get; }
        public bool IsAttackingGroup { get; }
        public bool IsPartyMember { get; }
        public WoWClass WoWClass { get; }

        public uint GetBaseAdress { get; }

        public CachedWoWUnit(WoWUnit unit)
        {
            Name = unit.Name;
            Guid = unit.Guid;
            TargetGuid = unit.Target;
            Valid = unit.IsValid;
            Dead = unit.IsDead;
            PositionWithoutType = unit.PositionWithoutType;
            HealthPercent = unit.HealthPercent;
            ManaPercent = unit.ManaPercentage;
            RagePercent = unit.RagePercentage;
            FocusPercent = unit.FocusPercentage;
            InCombatFlagOnly = unit.InCombatFlagOnly;
            UnitFlags = unit.UnitFlags;
            IsLootable = unit.IsLootable;
            IsAttackingGroup = unit.IsTargetingPartyMember;
            IsPartyMember = unit.IsPartyMember;
            WoWClass = unit.WowClass;
            GetBaseAdress = unit.GetBaseAddress;


            var auras = new Dictionary<uint, IAura>();
            foreach (var aura in BuffManager.GetAuras(unit.GetBaseAddress))
            {
                auras[aura.SpellId] = new CachedAura(aura);
            }
            Auras = auras;
        }
    }

}
