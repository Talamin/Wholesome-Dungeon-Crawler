using robotManager.Helpful;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    internal class BannedSafeZone
    {
        private readonly Timer _timer;
        public Vector3 Position { get; private set; }
        public bool ShouldBeRemoved => _timer.IsReady;

        public BannedSafeZone(Vector3 position)
        {
            Position = position;
            _timer = new Timer(10 * 1000);
        }
    }
}
