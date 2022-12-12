namespace WholesomeDungeonCrawler.Helpers
{
    public enum LFGRoles
    {
        MDPS,
        RDPS,
        Tank,
        Heal
    }

    public enum LFGMode
    {
        abandonedInDungeon = 1, //LFG party disbanded, player still in dungeon.
        lfgparty = 2, //LFG dungeon in progress.
        proposal = 3, //LFG party formed, notifying matched players dungeon is ready (ready check).
        queued = 4, //Currently in LFG queue.
        rolecheck = 5, //Querying groupmates to select their LFG roles before queuing.
        nil = 6 //Not in LFG.
    }

    // The condition type is recorded in the profiles by index. Never reuse an obsolete index
    public enum CompleteConditionType
    {
        None = 0,
        Csharp = 1,
        FlagsChanged = 2,
        HaveItem = 3,
        MobDead = 4,
        MobPosition = 5,
        LOSCheck = 6,
        CantGossip = 7,
    }
}
