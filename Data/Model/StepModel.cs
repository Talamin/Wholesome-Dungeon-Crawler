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
        [JsonProperty("$type")]
        public string Type { get;}
        public bool IsCompleted { get; set; }
        public string Name { get; set; }
        public object Order { get; set; }
    }

}
