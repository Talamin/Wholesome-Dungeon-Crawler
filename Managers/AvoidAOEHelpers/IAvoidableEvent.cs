using robotManager.Helpful;
using WholesomeDungeonCrawler.Managers.AvoidAOEHelpers;

namespace WholesomeDungeonCrawler.Managers.ManagedEvents
{
    public interface IAvoidableEvent
    {
        bool PositionInDanger(Vector3 position, DangerZone zone, int margin = 0);
        //void Draw(Vector3 position, DangerZone zone, Color color, bool filled, int alpha);
    }
}
