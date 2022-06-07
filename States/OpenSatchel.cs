using robotManager.FiniteStateMachine;
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

        public OpenSatchel(ICache iCache)
        {
            _cache = iCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !ObjectManager.Me.IsValid
                    || Fight.InFight
                    || _cache.IsInInstance)
                {
                    return false;
                }

                return _cache.HaveSatchel;
            }
        }

        public override void Run()
        {
            WoWItem item = Bag.GetBagItem().FirstOrDefault(x => x.Name.Contains("Satchel of"));
            if (item != null)
            {
                Logger.Log($"Found Satchel {item}");
                ItemsManager.UseItem(item.Name);
            }
        }
    }
}
