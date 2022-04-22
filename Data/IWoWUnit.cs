using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wManager.Wow.Enums;

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

    }
}
