namespace Roguelike.Domain.Gameplay.Items.Enums
{
    /// <summary>
    /// アイテム識別子です。
    /// 旧 RoguelikeItemType / SpellKey を統合し、
    /// 通常アイテムと魔法書を同一軸で扱います。
    /// </summary>
    public enum ItemId
    {
        /// <summary>
        /// 空腹度を回復する食料。
        /// </summary>
        FoodRation,

        /// <summary>
        /// HPを回復するポーション。
        /// </summary>
        HealingPotion,

        /// <summary>
        /// 防御力補正を持つ装備。
        /// </summary>
        Armor,

        /// <summary>
        /// 直線攻撃魔法を内包する魔法書。
        /// </summary>
        SpellbookForceBolt,

        /// <summary>
        /// 近距離火炎魔法を内包する魔法書。
        /// </summary>
        SpellbookMagicFire,

        /// <summary>
        /// 睡眠付与魔法を内包する魔法書。
        /// </summary>
        SpellbookSleep,

        /// <summary>
        /// 防御支援魔法を内包する魔法書。
        /// </summary>
        SpellbookShield,

        /// <summary>
        /// 瞬間移動魔法を内包する魔法書。
        /// </summary>
        SpellbookBlink,

        /// <summary>
        /// 索敵支援魔法を内包する魔法書。
        /// </summary>
        SpellbookDetect
    }
}


