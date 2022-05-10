namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public abstract class Step : IStep
    {
        public bool IsCompleted { get; protected set; }

        public abstract void Run();
    }
}
