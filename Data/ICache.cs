using System.Collections.Generic;

namespace WholesomeDungeonCrawler.Data
{
    interface ICache : ICycleable
    {
        bool IsInInstance { get; }
        bool IsPartyInviteRequest { get; }
        bool HaveSatchel { get; }
        List<string> ListPartyMember { get; }
        string GetLFGMode { get; }
        bool MiniMapLFGFrameIcon { get; }
        string GetPlayerSpec { get; }
        bool LFGProposalShown { get; }
        bool LFGRoleCheckShown { get; }
    }
}
