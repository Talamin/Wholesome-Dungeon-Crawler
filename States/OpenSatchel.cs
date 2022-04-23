using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class OpenSatchel : State, IState
    {
        public override string DisplayName => "Open Satchel";
        private readonly ICache _cache;

        public OpenSatchel(ICache iCache, int priority)
        {
            _cache = iCache;
            Priority = priority;
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
