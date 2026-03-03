using System;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.Inventory.Services
{
    /// <summary>
    /// InventoryReadModelServiceAdditional の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class InventoryReadModelServiceAdditionalTests
    {
        // 観点: TryGetInventoryItem_ReturnsFalse_WhenItemIdIsEmpty の期待挙動を検証する。
        [Fact]
        public void TryGetInventoryItem_ReturnsFalse_WhenItemIdIsEmpty()
        {
            var sut = new InventoryReadModelService(new ApplicationTestFactory.SpyRunStore(ApplicationTestFactory.CreateRunSession()));

            var found = sut.TryGetInventoryItem(Guid.Empty, out _);

            Assert.False(found);
        }

        // 観点: TryGetInventoryItem_ReturnsFalse_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void TryGetInventoryItem_ReturnsFalse_WhenRunIsMissing()
        {
            var sut = new InventoryReadModelService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetInventoryItem(Guid.NewGuid(), out _);

            Assert.False(found);
        }

        // 観点: TryGetInventoryItems_ReturnsTrueWithEmpty_WhenInventoryIsEmpty の期待挙動を検証する。
        [Fact]
        public void TryGetInventoryItems_ReturnsTrueWithEmpty_WhenInventoryIsEmpty()
        {
            var sut = new InventoryReadModelService(new ApplicationTestFactory.SpyRunStore(ApplicationTestFactory.CreateRunSession()));

            var found = sut.TryGetInventoryItems(out var items);

            Assert.True(found);
            Assert.Empty(items);
        }

        // 観点: TryGetSpellPreviewContext_ReturnsFalse_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void TryGetSpellPreviewContext_ReturnsFalse_WhenRunIsMissing()
        {
            var sut = new InventoryReadModelService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetSpellPreviewContext(Guid.NewGuid(), out _);

            Assert.False(found);
        }

        // 観点: TryGetSpellPreviewContext_ReturnsFalse_WhenItemIsNotInInventory の期待挙動を検証する。
        [Fact]
        public void TryGetSpellPreviewContext_ReturnsFalse_WhenItemIsNotInInventory()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            ApplicationTestFactory.AddInventoryItem(run.Player, ItemId.SpellbookMagicFire);
            var sut = new InventoryReadModelService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetSpellPreviewContext(Guid.NewGuid(), out _);

            Assert.False(found);
        }
    }
}
