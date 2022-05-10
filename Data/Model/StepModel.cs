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
        public StepType StepType { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }
    public class StepType
    {
        public string StepTypeName => this.GetType().Name;
    }

    class Execute : StepType
    {
        public string Action { get; set; }
        public bool CheckCompletion { get; set; }
    }

    class GoTo : StepType
    {
        public float Precision { get; set; }
        public Vector3 TargetPosition { get; set; }
    }

    class MoveAlongPath : StepType
    {
        public List<Vector3> Path { get; set; }
    }

    class MoveToUnit : StepType
    {
        public Vector3 ExpectedPosition { get; set; }
        public bool FindClosest { get; set; }
        public bool SkipIfNotFound { get; set; }
        public int UnitId { get; set; }
        public bool Interactwithunit { get; set; }
        public int Gossip { get; set; }
    }

    class PickupObject : StepType
    {
        public int ObjectId { get; set; }
        public uint ItemId { get; set; }
        public Vector3 ExpectedPosition { get; set; }
        public bool FindClosest { get; set; }
        public bool StrictPosition { get; set; }
        public float InteractDistance { get; set; }
    }

    class InteractWith : StepType
    {
        public int ObjectId { get; set; }
        public Vector3 ExpectedPosition { get; set; }
    }


}
