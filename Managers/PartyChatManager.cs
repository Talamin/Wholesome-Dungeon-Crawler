using WholesomeDungeonCrawler.Profiles.Steps;

namespace WholesomeDungeonCrawler.Managers
{
    internal class PartyChatManager : IPartyChatManager
    {
        private RegroupStep _regroupStep;
        private LeaveDungeonStep _leaveDungeonStep;

        //public PlayerStatus TankStatus { get; private set; }

        public PartyChatManager()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void SetRegroupStep(RegroupStep regroupStep)
        {
            _regroupStep = regroupStep;
        }

        public void SetLeaveDungeonStep(LeaveDungeonStep leaveDungeonStep)
        {
            _leaveDungeonStep = leaveDungeonStep;
        }

        public void PartyReadyReceived()
        {
            _regroupStep?.PartyReadyReceived();
            _leaveDungeonStep?.PartyReadyReceived();
        }
    }
}