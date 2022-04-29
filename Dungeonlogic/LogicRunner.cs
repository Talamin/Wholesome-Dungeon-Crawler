using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    // Make this class and its members not static, and add an interface
    internal class LogicRunner : ILogicRunner
    {
        private object ProfileLocker = new object();
        private Profile _currentProfile;

        public LogicRunner()
        {
        }

        public string CurrentState
        {
            get
            {
                return "[LogicRunner] " + (_currentProfile?.CurrentState ?? "No profile");
            }
        }

        public bool IsFinished => _currentProfile == null;

        public bool OverrideNeedToRun
        {
            get
            {
                return _currentProfile?.OverrideNeedToRun ?? false;
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


        public bool Pulse()
        {
            if (_currentProfile == null) return true;
            if (_currentProfile.Pulse())
            {
                Logger.Log($"Finished {_currentProfile.Name} profile.");
                _currentProfile = null;
                return true;
            }
            return false;
        }
    }
}
