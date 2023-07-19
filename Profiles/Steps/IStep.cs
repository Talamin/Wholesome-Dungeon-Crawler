using WholesomeDungeonCrawler.Models;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public interface IStep
    {
        bool IsCompleted { get; }
        abstract string Name { get; }
        StepCompleteConditionModel StepCompleteConditionModel { get; }

        void Run();
        void MarkAsCompleted();
        void Initialize();
        void Dispose();
    }
}
