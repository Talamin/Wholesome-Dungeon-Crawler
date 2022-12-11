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

    public enum CompleteConditionType
    {
        None,
        Csharp,
        FlagsChanged,
        HaveItem,
        MobDead,
        MobPosition,
        LOSCheck,
        //WaitUntil,
        CantGossip,
    }
}
