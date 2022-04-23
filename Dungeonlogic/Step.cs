using System;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    // add and interface
    internal abstract class Step
    {
        public bool IsCompleted;
        public virtual bool OverrideNeedToRun => false;

        protected Step(string stepName = "Unnamed")
        {
            Name = stepName;
        }

        public string Name { get; }

        public virtual bool Pulse() => throw new NotImplementedException();
    }
}
