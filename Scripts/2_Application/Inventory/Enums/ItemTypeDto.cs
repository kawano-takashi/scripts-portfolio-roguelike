namespace Roguelike.Application.Enums
{
    /// <summary>
    /// Application層で扱うアイテム種別です。
    /// Domain.ItemId と同じ並びを維持します。
    /// </summary>
    public enum ItemTypeDto
    {
        FoodRation = 0,
        HealingPotion = 1,
        Armor = 2,
        SpellbookForceBolt = 3,
        SpellbookMagicFire = 4,
        SpellbookSleep = 5,
        SpellbookShield = 6,
        SpellbookBlink = 7,
        SpellbookDetect = 8
    }

    public static class ItemTypeDtoExtensions
    {
        public static bool IsSpellbook(this ItemTypeDto itemType)
        {
            return itemType >= ItemTypeDto.SpellbookForceBolt;
        }
    }
}
