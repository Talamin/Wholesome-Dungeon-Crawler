using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache.Entity
{
    internal class CachedWoWUnit : IWoWUnit
    {
        public string Name { get; }
        public int UnitID { get; }
        public ulong Guid { get; }
        public ulong TargetGuid { get; }
        public bool Valid { get; }
        public bool Dead { get; }
        public Vector3 PositionWithoutType { get; }
        public double HealthPercent { get; }
        public long Health { get; }
        public long Mana { get; }
        public double ManaPercent { get; }
        public double RagePercent { get; }
        public double FocusPercent { get; }
        public bool InCombatFlagOnly { get; }
        public  Reaction Reaction { get; }
        public UnitFlags UnitFlags { get; }
        public IReadOnlyDictionary<uint, IAura> Auras { get; }   
        public bool IsLootable { get; }
        public bool IsAttackingGroup { get; }
        public bool IsAttackingMe { get; }
        public bool IsPartyMember { get; }
        public uint GetBaseAdress { get; }
        public bool Fleeing { get; }
        public WoWUnit WowUnit { get; }

        public CachedWoWUnit(WoWUnit unit)
        {
            Name = unit.Name;
            UnitID = unit.Entry;
            Guid = unit.Guid;
            Valid = unit.IsValid;
            Dead = unit.IsDead;
            PositionWithoutType = unit.PositionWithoutType;
            HealthPercent = unit.HealthPercent;
            Health = unit.Health;
            Mana = unit.Mana;
            ManaPercent = unit.ManaPercentage;
            RagePercent = unit.RagePercentage;
            FocusPercent = unit.FocusPercentage;
            InCombatFlagOnly = unit.InCombatFlagOnly;
            Reaction = unit.Reaction;
            UnitFlags = unit.UnitFlags;
            IsLootable = unit.IsLootable;
            IsAttackingMe = unit.IsTargetingMe;
            IsAttackingGroup = unit.IsTargetingPartyMember;
            IsPartyMember = unit.IsPartyMember;
            TargetGuid = unit.Target;
            Fleeing = unit.Fleeing;
            WowUnit = unit;

            GetBaseAdress = unit.GetBaseAddress;


            var auras = new Dictionary<uint, IAura>();
            foreach (var aura in BuffManager.GetAuras(unit.GetBaseAddress))
            {
                auras[aura.SpellId] = new CachedAura(aura);
            }
            Auras = auras;
        }

        private HashSet<uint> _drinkBuffs = new HashSet<uint>()
        {
            430, 431, 432, 833, 1133, 1135, 1137, 2639, 10250, 18071, 18140,
            18233, 22734, 23540, 23541, 23542, 23692, 23698, 24355, 24384,
            24409, 24410, 24411, 24707, 25690, 25691, 25692, 25693, 25697,
            25701, 25887, 25990, 26263, 27089, 29007, 29029, 29055, 33266,
            33772, 34291, 41031, 42308, 42309, 42312, 43154, 43182, 43183,
            44109, 44110, 44111, 44112, 44113, 44114, 44115, 44116, 44166,
            45019, 45020, 46755, 49472, 52911, 53373, 56439, 57070, 57085,
            57096, 57098, 57101, 57106, 57289, 57292, 57333, 57335, 57341,
            57343, 57344, 57354, 57359, 57364, 57366, 57370, 58067, 58503,
            58645, 58648, 61827, 61828, 61830, 64056, 64354, 65363, 65418,
            65419, 65420, 65421, 65422, 69560, 69561, 72623
        };

        public bool HasDrinkBuff => Auras.Any(aura => _drinkBuffs.Contains(aura.Key));

        private HashSet<uint> _foodBuffs = new HashSet<uint>()
        {
            433, 434, 435, 1127, 1129, 1131, 2639, 5004, 5005, 5006, 5007,
            7737, 9177, 10256, 10257, 18071, 18124, 18229, 18230, 18231,
            18232, 18233, 18234, 21149, 22731, 23540, 23541, 23542, 23692,
            24005, 24384, 24409, 24410, 24411, 24707, 24800, 24869, 25660,
            25690, 25691, 25692, 25693, 25697, 25700, 25886, 25990, 26030,
            26263, 27094, 28616, 29008, 29029, 29055, 29073, 32112, 33253,
            33255, 33258, 33260, 33262, 33264, 33266, 33269, 33725, 33772,
            35270, 35271, 40745, 40768, 41030, 41031, 42207, 42309, 42311,
            43180, 43763, 44166, 45548, 45618, 46683, 46812, 46898, 53283,
            56439, 57069, 57070, 57084, 57085, 57096, 57098, 57101, 57106,
            57110, 57138, 57285, 57287, 57289, 57292, 57324, 57326, 57328,
            57331, 57333, 57335, 57341, 57343, 57344, 57354, 57355, 57357,
            57359, 57362, 57364, 57366, 57370, 57372, 57649, 58067, 58503,
            58645, 58648, 58886, 59227, 61827, 61828, 61829, 61874, 62351,
            64056, 64354, 64355, 65418, 65419, 65420, 65421, 65422, 71068,
            71071, 71073, 71074
        };
        public bool HasFoodBuff => Auras.Any(aura => _foodBuffs.Contains(aura.Key));
    }

}
