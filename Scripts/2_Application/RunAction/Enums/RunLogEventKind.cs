namespace Roguelike.Application.Enums
{
    /// <summary>
    /// ログ化対象イベントの意味種別です。
    /// </summary>
    public enum RunLogEventKind
    {
        AttackDeclared,
        SpellCast,
        ActorDamaged,
        ActorDied,
        ActorHealed,
        ExperienceGained,
        LevelUp,
        ItemAddedToInventory,
        ItemUsed,
        ItemDropped,
        ItemEquipped,
        ItemUnequipped,
        RunCleared,
        RunGameOver,
        Message
    }
}
