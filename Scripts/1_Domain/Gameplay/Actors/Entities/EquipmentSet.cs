using System;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Actors.Entities
{
    public sealed class EquipmentSet
    {
        // 装備中の魔法書のみを保持（ステータス補正は付与しない）
        public ItemInstanceId? SpellbookItemId { get; private set; }
        public ItemInstanceId? ArmorItemId { get; private set; }

        public bool IsEquipped(ItemInstanceId itemId)
        {
            return (SpellbookItemId.HasValue && SpellbookItemId.Value == itemId) ||
                   (ArmorItemId.HasValue && ArmorItemId.Value == itemId);
        }

        public bool TryEquip(InventoryItem item, out ItemInstanceId? replacedItemId, out EquipmentSlot slot)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var definition = ItemCatalog.GetDefinition(item.ItemType);
            if (!definition.IsEquippable)
            {
                replacedItemId = null;
                slot = EquipmentSlot.None;
                return false;
            }

            switch (definition.EquipSlot)
            {
                case EquipmentSlot.Spellbook:
                    replacedItemId = SpellbookItemId;
                    SpellbookItemId = item.Id;
                    slot = EquipmentSlot.Spellbook;
                    return true;
                case EquipmentSlot.Armor:
                    replacedItemId = ArmorItemId;
                    ArmorItemId = item.Id;
                    slot = EquipmentSlot.Armor;
                    return true;
                default:
                    replacedItemId = null;
                    slot = EquipmentSlot.None;
                    return false;
            }
        }

        public bool TryUnequip(ItemInstanceId itemId, out EquipmentSlot slot)
        {
            if (SpellbookItemId.HasValue && SpellbookItemId.Value == itemId)
            {
                SpellbookItemId = null;
                slot = EquipmentSlot.Spellbook;
                return true;
            }

            if (ArmorItemId.HasValue && ArmorItemId.Value == itemId)
            {
                ArmorItemId = null;
                slot = EquipmentSlot.Armor;
                return true;
            }

            slot = EquipmentSlot.None;
            return false;
        }

        public ActorStatModifier GetTotalModifier(Inventory inventory)
        {
            if (inventory == null)
            {
                return ActorStatModifier.None;
            }

            var total = ActorStatModifier.None;

            // 魔法書は補正なし。防具のみ補正を合算する。
            if (ArmorItemId.HasValue && inventory.TryGetById(ArmorItemId.Value, out var armorItem))
            {
                total += GetModifierForItem(armorItem.ItemType);
            }

            return total;
        }

        public bool TryGetEquippedSpellbook(Inventory inventory, out InventoryItem spellbookItem)
        {
            spellbookItem = null;
            if (!SpellbookItemId.HasValue || inventory == null)
            {
                return false;
            }

            // 呼び出し側で Spell を参照できるよう、インベントリアイテムを返す。
            return inventory.TryGetById(SpellbookItemId.Value, out spellbookItem);
        }

        private static ActorStatModifier GetModifierForItem(ItemId itemType)
        {
            var definition = ItemCatalog.GetDefinition(itemType);
            return new ActorStatModifier(definition.AttackBonus, definition.DefenseBonus);
        }
    }
}

