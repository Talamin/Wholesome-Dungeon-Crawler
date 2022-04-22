using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data
{
    internal interface IEntityCache
    {
        IWoWUnit Target { get; }
        IWoWUnit Pet { get; }

        IWoWUnit[] EnemyUnitsNearTarget { get; }
        IWoWUnit[] EnemyUnitsNearPlayer { get; }
        IWoWUnit[] InterruptibleEnemyUnits { get; }
        IWoWUnit[] EnemyUnitsTargetingPlayer { get; }
        IWoWUnit[] EnemyUnitsTargetingGroup { get; }
        IWoWUnit[] EnemyUnitsLootable { get; }
        IWoWUnit[] EnemyAttackingGroup { get; }
    }

}
