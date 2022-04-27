using System;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    // add and interface
    public abstract class Step
    {
        public bool IsCompleted;
        public virtual bool OverrideNeedToRun => false;

        protected Step(string stepName = "Unnamed")
        {
            Name = stepName;
        }

        public string Name { get; }
        public string Order { get; set; }

        public virtual bool Pulse() => throw new NotImplementedException();
    }
}
