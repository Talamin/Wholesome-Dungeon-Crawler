using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class OpenSatchel : State, IState
    {
        public override string DisplayName => "Open Satchel";
        private readonly ICache _cache;
        private Timer _stateTimer = new Timer();

        public OpenSatchel(ICache iCache)
        {
            _cache = iCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_stateTimer.IsReady
                    || _cache.IsInInstance
                    || !Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight)
                {
                    return false;
                }

                _stateTimer = new Timer(5000);

                return Bag.GetBagItem().Exists(item => item.Name.Contains("Satchel of"));
            }
        }

        public override void Run()
        {
            WoWItem item = Bag.GetBagItem().FirstOrDefault(x => x.Name.Contains("Satchel of"));
            if (item != null)
            {
                Logger.Log($"Opening {item.Name}");
                ItemsManager.UseItem(item.Name);
            }
        }
    }
}
