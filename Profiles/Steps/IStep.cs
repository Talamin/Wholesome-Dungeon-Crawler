namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public interface IStep
    {
        bool IsCompleted { get; }
        abstract string Name { get; }
        abstract int Order { get; }

        void Run();
        void MarkAsCompleted();
    }
}
