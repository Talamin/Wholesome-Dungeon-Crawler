using WholesomeDungeonCrawler.Data.Model;
using WholesomeDungeonCrawler.Profiles.Steps;

namespace WholesomeDungeonCrawler.Profiles
{
    public interface IProfile
    {
        IStep CurrentStep { get; }
        void SetCurrentStep();
    }
}
