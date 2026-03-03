using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Items.Services
{
    /// <summary>
    /// SpellEnhancementCalculator の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SpellEnhancementCalculatorTests
    {
        // 観点: CalculateDamageRange_AddsPowerLevelToMinAndMax の期待挙動を検証する。
        [Fact]
        public void CalculateDamageRange_AddsPowerLevelToMinAndMax()
        {
            var definition = ItemCatalog.GetDefinition(ItemId.SpellbookMagicFire);
            var enhancements = ItemEnhancements.Create((SpellEnhancementCategory.Power, 2));

            var (min, max) = SpellEnhancementCalculator.CalculateDamageRange(definition, enhancements);

            Assert.Equal(5, min);
            Assert.Equal(7, max);
        }

        // 観点: CalculateRange_AddsRangeEnhancement の期待挙動を検証する。
        [Fact]
        public void CalculateRange_AddsRangeEnhancement()
        {
            var definition = ItemCatalog.GetDefinition(ItemId.SpellbookSleep);
            var enhancements = ItemEnhancements.Create((SpellEnhancementCategory.Range, 3));

            var range = SpellEnhancementCalculator.CalculateRange(definition, enhancements);

            Assert.Equal(8, range);
        }

        // 観点: CalculateHungerCost_DoesNotGoBelowZero の期待挙動を検証する。
        [Fact]
        public void CalculateHungerCost_DoesNotGoBelowZero()
        {
            var definition = ItemCatalog.GetDefinition(ItemId.SpellbookMagicFire);
            var enhancements = ItemEnhancements.Create((SpellEnhancementCategory.Efficiency, 5));

            var cost = SpellEnhancementCalculator.CalculateHungerCost(definition, enhancements);

            Assert.Equal(0, cost);
        }

        // 観点: CalculateStatusTurns_AddsPowerEnhancement の期待挙動を検証する。
        [Fact]
        public void CalculateStatusTurns_AddsPowerEnhancement()
        {
            var definition = ItemCatalog.GetDefinition(ItemId.SpellbookSleep);
            var enhancements = ItemEnhancements.Create((SpellEnhancementCategory.Power, 1));

            var (min, max) = SpellEnhancementCalculator.CalculateStatusTurns(definition, enhancements);

            Assert.Equal(3, min);
            Assert.Equal(5, max);
        }

        // 観点: CalculateBlinkDistance_AddsPowerEnhancement の期待挙動を検証する。
        [Fact]
        public void CalculateBlinkDistance_AddsPowerEnhancement()
        {
            var definition = ItemCatalog.GetDefinition(ItemId.SpellbookBlink);
            var enhancements = ItemEnhancements.Create((SpellEnhancementCategory.Power, 2));

            var (min, max) = SpellEnhancementCalculator.CalculateBlinkDistance(definition, enhancements);

            Assert.Equal(4, min);
            Assert.Equal(6, max);
        }
    }
}
