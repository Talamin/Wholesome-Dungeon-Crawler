using robotManager.Helpful;
using System.Drawing;

namespace WholesomeDungeonCrawler.Managers.ManagedEvents
{
    public interface IAvoidableEvent
    {
        bool PositionInDanger(Vector3 playerPosition, Vector3 dangerPosition, int margin);

        void Draw(Vector3 position, Color color, bool filled, int alpha);
    }
}
