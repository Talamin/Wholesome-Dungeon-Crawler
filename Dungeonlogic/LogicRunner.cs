using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    // Make this class and its members not static, and add an interface
    internal class LogicRunner : ILogicRunner
    {
        private readonly object ProfileLocker = new object();
        private Profile _currentProfile;
        public bool Pulse{ get; set; }

        public LogicRunner()
        { 
        }

        public string CurrentState
        {
            get
            {
                lock (ProfileLocker)
                {
                    return "[LogicRunner] " + (_currentProfile?.CurrentState ?? "No profile");
                }
            }
        }

        public bool IsFinished => _currentProfile == null;

        public bool OverrideNeedToRun
        {
            get
            {
                lock (ProfileLocker)
                {
                    return _currentProfile?.OverrideNeedToRun ?? false;
                }
            }
        }

        public void CheckUpdate(Profile profile)
        {
            lock (ProfileLocker)
            {
                if (_currentProfile == profile) return;
                _currentProfile = profile;
                _currentProfile.Reset();
            }

            Logger.Log($"[LogicRunner] Loaded new profile {profile.Name}.");
        }

        public void Pulses()
        {
            lock (ProfileLocker)
            {
                if (_currentProfile == null)
                {
                    Pulse = true;
                }
                if (_currentProfile.Pulse())
                {
                    Logger.Log($"Finished {_currentProfile.Name} profile.");
                    _currentProfile = null;
                    Pulse = true;
                }
            }
            Pulse = false;
        }
    }
}
