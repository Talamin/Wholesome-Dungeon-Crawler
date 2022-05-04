using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data.Model
{
    class StepModel
    {
        [JsonProperty("$type")]
        public string Type { get;}
        public bool IsCompleted { get; }
        public List<Vector3> Path { get; }
        public double Randomization { get; }
        public object Target { get; }
        public bool OverrideNeedToRun { get;}
        public string Name { get;}
        public object Order { get; }
    }
}
