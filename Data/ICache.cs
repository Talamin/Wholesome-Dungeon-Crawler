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
        List<string> ListPartyMember { get; }
        string GetLFGMode { get; }
    }
}
