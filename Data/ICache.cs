using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholesomeDungeonCrawler.Data
{
    interface ICache
    {
        bool IsInInstance { get;}
        bool IsPartyInviteRequest { get; }
        bool HaveSatchel { get; }
        bool InParty { get; }
        string PartymemberName { get; set; }
    }
}
