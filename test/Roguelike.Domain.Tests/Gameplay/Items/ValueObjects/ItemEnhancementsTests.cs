using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Items.ValueObjects
{
    /// <summary>
    /// ItemEnhancements の仕様を検証するユニットテストです。
    /// </summary>
    public sealed class ItemEnhancementsTests
    {
        // 観点: Levels_ReturnsReadOnlyDictionary の期待挙動を検証する。
        [Fact]
        public void Levels_ReturnsReadOnlyDictionary()
        {
            var sut = ItemEnhancements.Create((SpellEnhancementCategory.Power, 1));
            var levels = Assert.IsAssignableFrom<IDictionary<SpellEnhancementCategory, int>>(sut.Levels);

            Assert.Throws<NotSupportedException>(() => levels.Add(SpellEnhancementCategory.Range, 2));
            Assert.Equal(1, sut.GetLevel(SpellEnhancementCategory.Power));
            Assert.Equal(0, sut.GetLevel(SpellEnhancementCategory.Range));
        }

        // 観点: None_StaysEmpty_WhenModificationIsAttempted の期待挙動を検証する。
        [Fact]
        public void None_StaysEmpty_WhenModificationIsAttempted()
        {
            var levels = Assert.IsAssignableFrom<IDictionary<SpellEnhancementCategory, int>>(ItemEnhancements.None.Levels);

            Assert.Throws<NotSupportedException>(() => levels.Add(SpellEnhancementCategory.Power, 1));
            Assert.False(ItemEnhancements.None.HasAny);
            Assert.Equal(0, ItemEnhancements.None.GetLevel(SpellEnhancementCategory.Power));
        }
    }
}
