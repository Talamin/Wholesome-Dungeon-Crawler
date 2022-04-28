using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    interface ILogicRunner
    {
        string CurrentState { get; }
        bool IsFinished { get; }
        bool OverrideNeedToRun { get; }
        void CheckUpdate(Profile profile);
        bool Pulse();
    }
}
