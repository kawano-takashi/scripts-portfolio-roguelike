using System.Collections.Generic;
using System.Linq;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Items.Services
{
    /// <summary>
    /// ItemCatalog の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class ItemCatalogTests
    {
        // 観点: GetDefinition_Throws_WhenItemIsNotRegistered の期待挙動を検証する。
        [Fact]
        public void GetDefinition_Throws_WhenItemIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => ItemCatalog.GetDefinition((ItemId)999));
        }

        // 観点: TryGetDefinition_ReturnsFalse_WhenItemIsNotRegistered の期待挙動を検証する。
        [Fact]
        public void TryGetDefinition_ReturnsFalse_WhenItemIsNotRegistered()
        {
            var result = ItemCatalog.TryGetDefinition((ItemId)999, out var definition);

            Assert.False(result);
            Assert.Null(definition);
        }

        // 観点: TryGetSpellDefinition_ReturnsTrue_OnlyForSpellbooks の期待挙動を検証する。
        [Fact]
        public void TryGetSpellDefinition_ReturnsTrue_OnlyForSpellbooks()
        {
            var isSpellbook = ItemCatalog.TryGetSpellDefinition(ItemId.SpellbookMagicFire, out var spell);
            var isArmorSpellbook = ItemCatalog.TryGetSpellDefinition(ItemId.Armor, out var armor);

            Assert.True(isSpellbook);
            Assert.True(spell.IsSpellbook);
            Assert.False(isArmorSpellbook);
            Assert.False(armor.IsSpellbook);
        }

        // 観点: GetSpellbookDropPool_ReturnsNonEmptySpellbookList の期待挙動を検証する。
        [Fact]
        public void GetSpellbookDropPool_ReturnsNonEmptySpellbookList()
        {
            var pool = ItemCatalog.GetSpellbookDropPool();

            Assert.NotEmpty(pool);
            Assert.All(pool, itemId => Assert.True(ItemCatalog.GetDefinition(itemId).IsSpellbook));
            Assert.True(pool.Distinct().Count() == pool.Count);
        }
    }
}
