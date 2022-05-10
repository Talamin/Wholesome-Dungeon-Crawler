namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public interface IStep
    {
        bool IsCompleted { get; }

        void Run();
    }
}
