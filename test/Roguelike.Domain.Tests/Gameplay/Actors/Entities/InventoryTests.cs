using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Xunit;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// Inventory の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class InventoryTests
    {
        // 観点: Constructor_Throws_WhenCapacityIsNotPositive の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenCapacityIsNotPositive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Inventory(0));
        }

        // 観点: TryAdd_Throws_WhenItemIsNull の期待挙動を検証する。
        [Fact]
        public void TryAdd_Throws_WhenItemIsNull()
        {
            var sut = new Inventory();

            Assert.Throws<ArgumentNullException>(() => sut.TryAdd(item: null));
        }

        // 観点: TryAdd_ReturnsFalse_WhenInventoryIsFull の期待挙動を検証する。
        [Fact]
        public void TryAdd_ReturnsFalse_WhenInventoryIsFull()
        {
            var sut = new Inventory(maxCapacity: 1);
            sut.TryAdd(new InventoryItem(ItemInstanceId.NewId(), ItemId.FoodRation));

            var added = sut.TryAdd(new InventoryItem(ItemInstanceId.NewId(), ItemId.HealingPotion));

            Assert.False(added);
            Assert.Equal(1, sut.Count);
        }

        // 観点: GetAt_ReturnsNull_WhenIndexIsOutOfRange の期待挙動を検証する。
        [Fact]
        public void GetAt_ReturnsNull_WhenIndexIsOutOfRange()
        {
            var sut = new Inventory(maxCapacity: 2);
            sut.TryAdd(new InventoryItem(ItemInstanceId.NewId(), ItemId.FoodRation));

            Assert.Null(sut.GetAt(-1));
            Assert.Null(sut.GetAt(10));
        }

        // 観点: TryGetById_ReturnsTrue_WhenItemExists の期待挙動を検証する。
        [Fact]
        public void TryGetById_ReturnsTrue_WhenItemExists()
        {
            var sut = new Inventory();
            var item = new InventoryItem(ItemInstanceId.NewId(), ItemId.Armor);
            sut.TryAdd(item);

            var found = sut.TryGetById(item.Id, out var actual);

            Assert.True(found);
            Assert.Same(item, actual);
        }

        // 観点: TryRemoveById_RemovesItem_WhenItemExists の期待挙動を検証する。
        [Fact]
        public void TryRemoveById_RemovesItem_WhenItemExists()
        {
            var sut = new Inventory();
            var item = new InventoryItem(ItemInstanceId.NewId(), ItemId.HealingPotion);
            sut.TryAdd(item);

            var removed = sut.TryRemoveById(item.Id, out var actual);

            Assert.True(removed);
            Assert.Same(item, actual);
            Assert.Equal(0, sut.Count);
            Assert.False(sut.TryGetById(item.Id, out _));
        }

        // 観点: TryRemoveAt_ReturnsFalse_WhenIndexIsOutOfRange の期待挙動を検証する。
        [Fact]
        public void TryRemoveAt_ReturnsFalse_WhenIndexIsOutOfRange()
        {
            var sut = new Inventory();

            var removed = sut.TryRemoveAt(0, out var item);

            Assert.False(removed);
            Assert.Null(item);
        }

        // 観点: Items_ReturnsReadOnlyCollection の期待挙動を検証する。
        [Fact]
        public void Items_ReturnsReadOnlyCollection()
        {
            var sut = new Inventory();
            var items = Assert.IsAssignableFrom<IList<InventoryItem>>(sut.Items);

            Assert.Throws<NotSupportedException>(() => items.Add(new InventoryItem(ItemInstanceId.NewId(), ItemId.FoodRation)));
        }
    }
}
