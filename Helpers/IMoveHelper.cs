using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Helpers
{
    interface IMoveHelper
    {
        Vector3 CurrentTarget { get;}
        bool IsMovementThreadRunning { get;}
        void StartGoToThread(Vector3 target, string log = null);
    }
}
