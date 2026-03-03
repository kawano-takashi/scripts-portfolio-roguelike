using System;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Items.Services
{
    /// <summary>
    /// 魔法書の強化レベルに応じた呪文パラメータ補正を計算するサービスです。
    /// </summary>
    public static class SpellEnhancementCalculator
    {
        /// <summary>
        /// 強化込みのダメージ範囲を計算します。
        /// Power Lv N の場合、最小/最大ダメージに N を加算します。
        /// </summary>
        public static (int min, int max) CalculateDamageRange(
            ItemDefinition definition, ItemEnhancements enhancements)
        {
            var powerBonus = enhancements.GetLevel(SpellEnhancementCategory.Power);
            var min = definition.SpellDamageMin.GetValueOrDefault() + powerBonus;
            var max = definition.SpellDamageMax.GetValueOrDefault(
                definition.SpellDamageMin.GetValueOrDefault()) + powerBonus;
            return (min, max);
        }

        /// <summary>
        /// 強化込みの射程を計算します。
        /// Range Lv N の場合、基礎射程に N を加算します。
        /// </summary>
        public static int CalculateRange(ItemDefinition definition, ItemEnhancements enhancements)
        {
            var rangeBonus = enhancements.GetLevel(SpellEnhancementCategory.Range);
            return definition.SpellRange.GetValueOrDefault() + rangeBonus;
        }

        /// <summary>
        /// 強化込みの空腹度コストを計算します。
        /// Efficiency Lv N の場合、基礎コストから N を減算します（下限0）。
        /// </summary>
        public static int CalculateHungerCost(ItemDefinition definition, ItemEnhancements enhancements)
        {
            var efficiencyBonus = enhancements.GetLevel(SpellEnhancementCategory.Efficiency);
            return Math.Max(0, definition.SpellBaseHungerCost.GetValueOrDefault() - efficiencyBonus);
        }

        /// <summary>
        /// 強化込みの状態異常ターン数を計算します。
        /// Power Lv N の場合、最小/最大ターン数に N を加算します。
        /// </summary>
        public static (int min, int max) CalculateStatusTurns(
            ItemDefinition definition, ItemEnhancements enhancements)
        {
            var powerBonus = enhancements.GetLevel(SpellEnhancementCategory.Power);
            var min = definition.SpellStatusTurnsMin.GetValueOrDefault() + powerBonus;
            var max = definition.SpellStatusTurnsMax.GetValueOrDefault(
                definition.SpellStatusTurnsMin.GetValueOrDefault()) + powerBonus;
            return (min, max);
        }

        /// <summary>
        /// 強化込みのブリンク距離を計算します。
        /// Power Lv N の場合、最小/最大距離に N を加算します。
        /// </summary>
        public static (int min, int max) CalculateBlinkDistance(
            ItemDefinition definition, ItemEnhancements enhancements)
        {
            var powerBonus = enhancements.GetLevel(SpellEnhancementCategory.Power);
            var min = definition.SpellBlinkMinDistance.GetValueOrDefault() + powerBonus;
            var max = definition.SpellBlinkMaxDistance.GetValueOrDefault(
                definition.SpellBlinkMinDistance.GetValueOrDefault()) + powerBonus;
            return (min, max);
        }
    }
}


