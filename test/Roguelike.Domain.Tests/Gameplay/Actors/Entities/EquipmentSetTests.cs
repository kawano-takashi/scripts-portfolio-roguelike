using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Xunit;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// EquipmentSet の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class EquipmentSetTests
    {
        // 観点: TryEquip_ReturnsFalse_WhenItemIsNotEquippable の期待挙動を検証する。
        [Fact]
        public void TryEquip_ReturnsFalse_WhenItemIsNotEquippable()
        {
            var sut = new EquipmentSet();
            var food = new InventoryItem(ItemInstanceId.NewId(), ItemId.FoodRation);

            var equipped = sut.TryEquip(food, out var replaced, out var slot);

            Assert.False(equipped);
            Assert.Null(replaced);
            Assert.Equal(EquipmentSlot.None, slot);
        }

        // 観点: TryEquip_ReplacesSpellbook_WhenAlreadyEquipped の期待挙動を検証する。
        [Fact]
        public void TryEquip_ReplacesSpellbook_WhenAlreadyEquipped()
        {
            var sut = new EquipmentSet();
            var first = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookMagicFire);
            var second = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookSleep);

            Assert.True(sut.TryEquip(first, out var firstReplaced, out var firstSlot));
            Assert.Null(firstReplaced);
            Assert.Equal(EquipmentSlot.Spellbook, firstSlot);

            Assert.True(sut.TryEquip(second, out var secondReplaced, out var secondSlot));
            Assert.Equal(first.Id, secondReplaced);
            Assert.Equal(EquipmentSlot.Spellbook, secondSlot);
            Assert.Equal(second.Id, sut.SpellbookItemId);
        }

        // 観点: TryUnequip_ReturnsFalse_WhenItemIsNotEquipped の期待挙動を検証する。
        [Fact]
        public void TryUnequip_ReturnsFalse_WhenItemIsNotEquipped()
        {
            var sut = new EquipmentSet();

            var unequipped = sut.TryUnequip(ItemInstanceId.NewId(), out var slot);

            Assert.False(unequipped);
            Assert.Equal(EquipmentSlot.None, slot);
        }

        // 観点: GetTotalModifier_AppliesArmorBonusOnly の期待挙動を検証する。
        [Fact]
        public void GetTotalModifier_AppliesArmorBonusOnly()
        {
            var inventory = new Inventory();
            var armor = new InventoryItem(ItemInstanceId.NewId(), ItemId.Armor);
            var spellbook = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookMagicFire);
            inventory.TryAdd(armor);
            inventory.TryAdd(spellbook);

            var sut = new EquipmentSet();
            sut.TryEquip(armor, out _, out _);
            sut.TryEquip(spellbook, out _, out _);

            var modifier = sut.GetTotalModifier(inventory);

            Assert.Equal(0, modifier.Attack);
            Assert.Equal(2, modifier.Defense);
        }

        // 観点: TryGetEquippedSpellbook_ReturnsTrue_WhenSpellbookIsEquipped の期待挙動を検証する。
        [Fact]
        public void TryGetEquippedSpellbook_ReturnsTrue_WhenSpellbookIsEquipped()
        {
            var inventory = new Inventory();
            var spellbook = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookMagicFire);
            inventory.TryAdd(spellbook);

            var sut = new EquipmentSet();
            sut.TryEquip(spellbook, out _, out _);

            var found = sut.TryGetEquippedSpellbook(inventory, out var actual);

            Assert.True(found);
            Assert.Same(spellbook, actual);
        }

        // 観点: TryGetEquippedSpellbook_ReturnsFalse_WhenInventoryDoesNotContainItem の期待挙動を検証する。
        [Fact]
        public void TryGetEquippedSpellbook_ReturnsFalse_WhenInventoryDoesNotContainItem()
        {
            var inventory = new Inventory();
            var sut = new EquipmentSet();
            var spellbook = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookMagicFire);
            sut.TryEquip(spellbook, out _, out _);

            var found = sut.TryGetEquippedSpellbook(inventory, out var actual);

            Assert.False(found);
            Assert.Null(actual);
        }
    }
}

