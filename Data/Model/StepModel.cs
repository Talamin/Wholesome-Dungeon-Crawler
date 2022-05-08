using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data.Model
{
    public class StepModel
    {       
        public string StepType { get; set; }
        public bool IsCompleted { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

}
