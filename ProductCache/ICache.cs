namespace WholesomeDungeonCrawler.ProductCache
{
    interface ICache : ICycleable
    {
        bool IsInInstance { get; }
        string CurrentState { get; }
        bool LFGProposalShown { get; }
        bool LFGRoleCheckShown { get; }
        bool LootRollShow { get; }
        public bool IAmAlliance { get; }

        bool IsRunningForcedTownRun { get; set; }

        void CacheIsInInstance();
        void CacheLFGProposalShown();
        void CacheRoleCheckShow();
        void CacheLootRollShow();
    }
}
