using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    public interface IStep
    {
        bool IsCompleted { get; }
        abstract string Name { get; }
        StepCompleteConditionModel StepCompleteConditionModel { get; }
        abstract FactionType StepFaction { get; }
        abstract LFGRoles StepRole { get; }

        void Run();
        void MarkAsCompleted();
        void Initialize();
        void Dispose();
    }
}
