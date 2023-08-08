namespace WholesomeDungeonCrawler.Helpers
{
    // LFGRoles indexes are saved in profiles Never reuse obsolete role
    public enum LFGRoles
    {
        Unspecified = 0,
        MDPS = 1,
        RDPS = 2,
        Tank = 3,
        Heal = 4
    }
    /*
    public enum LFGMode
    {
        abandonedInDungeon = 1, //LFG party disbanded, player still in dungeon.
        lfgparty = 2, //LFG dungeon in progress.
        proposal = 3, //LFG party formed, notifying matched players dungeon is ready (ready check).
        queued = 4, //Currently in LFG queue.
        rolecheck = 5, //Querying groupmates to select their LFG roles before queuing.
        nil = 6 //Not in LFG.
    }
    */
    // The condition type is recorded in the profiles by index. Never reuse an obsolete index
    public enum CompleteConditionType
    {
        None = 0,
        FlagsChanged = 2,
        HaveItem = 3,
        MobDead = 4,
        MobAtPosition = 5,
        LOSCheck = 6,
        CanGossip = 7,
        MobAttackable = 8,
        Timer = 9
    }
}
