namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public interface IStep
    {
        bool IsCompleted { get; }
        abstract string Name { get; }

        void Run();
        void MarkAsCompleted();
    }
}
