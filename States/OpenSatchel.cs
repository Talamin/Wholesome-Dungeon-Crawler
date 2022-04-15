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
    class OpenSatchel : State
    {
        public override string DisplayName
        {
            get { return "Open Satchel"; }
        }
        public override int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private int _priority;

        private ICache _cache = new Cache();
        public override bool NeedToRun
        {
            get
            {
                if (_cache.IsInInstance)
                    return false;

                if (Bag.GetBagItem().Count(item => item.Name.Contains("Satchel of")) > 0)
                    return true;

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
