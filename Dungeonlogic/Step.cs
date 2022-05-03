using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    public abstract class Step
    {
        [JsonProperty("$type")]
        public string Type { get; set; }
        public bool IsCompleted { get; set; }
        public List<Vector3> Path { get; set; }
        public double Randomization { get; set; }
        public object Target { get; set; }
        public bool OverrideNeedToRun { get; set; }
        public string Name { get; set; }
        public object Order { get; set; }

    }
}
