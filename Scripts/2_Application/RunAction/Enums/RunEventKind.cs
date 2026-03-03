namespace Roguelike.Application.Enums
{
    /// <summary>
    /// ターン内イベントの意味種別です。
    /// </summary>
    public enum RunEventKind
    {
        Unknown = 0,
        AttackDeclared = 1,
        SpellCast = 2,
        AttackPerformed = 3,
        ActorDamaged = 4,
        ActorDied = 5,
        ActorHealed = 6,
        ExperienceGained = 7,
        LevelUp = 8,
        ItemAddedToInventory = 9,
        ItemUsed = 10,
        ItemDropped = 11,
        ItemEquipped = 12,
        ItemUnequipped = 13,
        ItemPicked = 14,
        ActorMoved = 15,
        ActorFacingChanged = 16,
        HungerChanged = 17,
        TurnEnded = 18,
        MonsterHouseTriggered = 19,
        Message = 20
    }
}
