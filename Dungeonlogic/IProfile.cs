using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    interface IProfile
    {
        ProfileModel ProfileModel { get; }
        StepModel CurrentStep { get; }
        void ExecuteSteps();

    }
}
