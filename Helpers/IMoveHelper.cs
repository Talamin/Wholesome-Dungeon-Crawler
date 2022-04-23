using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Helpers
{
    interface IMoveHelper
    {
        Vector3 CurrentTarget { get; set; }
        bool IsMovementThreadRunning { get; set; }
        void StartGoToThread(Vector3 target, string log = null);
    }
}
