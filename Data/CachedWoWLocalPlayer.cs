using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal sealed class CachedWoWLocalPlayer : CachedWoWPlayer, IWoWLocalPlayer
    {

        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {

        }
    }

}
