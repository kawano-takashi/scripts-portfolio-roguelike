namespace Roguelike.Domain.Gameplay.Items.Enums
{
    /// <summary>
    /// 魔法書の強化カテゴリです。
    /// 1つの魔法書が複数の軸を同時に持てます。
    /// </summary>
    public enum SpellEnhancementCategory
    {
        /// <summary>
        /// 威力（ダメージ/効果量）を強化します。
        /// </summary>
        Power,

        /// <summary>
        /// 射程を強化します。
        /// </summary>
        Range,

        /// <summary>
        /// 消費コスト（空腹度）を軽減します。
        /// </summary>
        Efficiency
    }
}


