using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Helpers
{
    interface IMoveHelper
    {
        Vector3 CurrentTarget { get; set; }
        bool IsMovementThreadRunning { get; set; }
        void StartGoToThread(Vector3 target, string log = null);
    }
}
