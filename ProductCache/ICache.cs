namespace WholesomeDungeonCrawler.ProductCache
{
    interface ICache : ICycleable
    {
        bool IsInInstance { get; }
        bool IsPartyInviteRequest { get; }
        bool HaveSatchel { get; }
        string CurrentState { get; }
        string GetLFGMode { get; }
        bool MiniMapLFGFrameIcon { get; }
        string GetPlayerSpec { get; }
        bool LFGProposalShown { get; }
        bool LFGRoleCheckShown { get; }
        bool LootRollShow { get; }
        public bool HaveResurrection { get; }
        public bool IAmAlliance { get; }
    }
}
