using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wholesome_Dungeon_Crawler.Helpers;
using WholesomeDungeonCrawler.Data;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.States
{
    class OpenSatchel : State, IState
    {
        public override string DisplayName
        {
            get { return "Open Satchel"; }
        }
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
                if (!Conditions.InGameAndConnected || !ObjectManager.Me.IsValid || Fight.InFight)
                {
                    return false;
                }
                if (_cache.IsInInstance)
                {
                    return false;
                }
                if (_cache.HaveSatchel)
                {
                    return true;
                }
                return false;
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
