using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data.Model.Stepmodel
{
    class ModelExecute : StepModel
    {
        public  Action action { get; set; }
        public bool CheckCompletion { get; set; }
    }
}
