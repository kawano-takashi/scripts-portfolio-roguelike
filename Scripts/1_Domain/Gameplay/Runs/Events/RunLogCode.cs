namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// ドメインログの意味コードです。
    /// 文言化は上位レイヤーで行います。
    /// </summary>
    public enum RunLogCode
    {
        None = 0,
        NoSpellbookEquipped = 1,
        SteppedOnItem = 2,
        PlayerDied = 3,
        LevelUp = 4,
        TooHungryToRest = 5,
        TooHungryToSearch = 6,
        SpellbookHasNoSpell = 7,
        Silenced = 8,
        NothingHappens = 9,
        TooHungryToCast = 10,
        SpellMiss = 11,
        NoTargetToSleep = 12,
        TargetFallsAsleep = 13,
        BlinkFailed = 14,
        Starving = 15,
        WakeUp = 16,
        NothingToPickUp = 17,
        InventoryFull = 18,
        ItemNotFoundInInventory = 19,
        ItemCannotBeUsed = 20,
        ItemCannotBeEquipped = 21,
        ItemAlreadyOnGround = 22,
        MonsterHouseTriggered = 23,
        RunIsNotActive = 24,
        InvalidPlayerAction = 25,
        ActorAsleep = 26
    }
}
