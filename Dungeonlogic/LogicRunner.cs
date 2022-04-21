using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    internal static class LogicRunner
    {
        private static readonly object ProfileLocker = new object();
        private static Profile _currentProfile;

        public static string CurrentState
        {
            get
            {
                lock (ProfileLocker)
                {
                    return "[LogicRunner] " + (_currentProfile?.CurrentState ?? "No profile");
                }
            }
        }

        public static bool IsFinished => _currentProfile == null;

        public static bool OverrideNeedToRun
        {
            get
            {
                lock (ProfileLocker)
                {
                    return _currentProfile?.OverrideNeedToRun ?? false;
                }
            }
        }

        public static void CheckUpdate(Profile profile)
        {
            lock (ProfileLocker)
            {
                if (_currentProfile == profile) return;
                _currentProfile = profile;
                _currentProfile.Reset();
            }

            Logger.Log($"[LogicRunner] Loaded new profile {profile.Name}.");
        }

        public static bool Pulse()
        {
            lock (ProfileLocker)
            {
                if (_currentProfile == null) return true;
                if (_currentProfile.Pulse())
                {
                    Logger.Log($"Finished {_currentProfile.Name} profile.");
                    _currentProfile = null;
                    return true;
                }
            }

            return false;
        }
    }
}
