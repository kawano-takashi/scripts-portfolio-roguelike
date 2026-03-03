using System;
using Roguelike.Domain.Gameplay.Items.Enums;

namespace Roguelike.Domain.Gameplay.Items.ValueObjects
{
    /// <summary>
    /// 1つのアイテム定義を表すマスタ値オブジェクトです。
    /// 魔法書の場合は、呪文パラメータも同じ定義に保持します。
    /// </summary>
    public sealed class ItemDefinition
    {
        public ItemId ItemType { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public ItemCategory Category { get; }
        public EquipmentSlot EquipSlot { get; }
        public int AttackBonus { get; }
        public int DefenseBonus { get; }
        public bool ConsumesOnUse { get; }
        // 以下は魔法書専用パラメータ。非魔法書では null を保持します。
        public string ShortSpellName { get; }
        public int? SpellRange { get; }
        public int? SpellBaseHungerCost { get; }
        public int? SpellDamageMin { get; }
        public int? SpellDamageMax { get; }
        public int? SpellStatusTurnsMin { get; }
        public int? SpellStatusTurnsMax { get; }
        public int? SpellBlinkMinDistance { get; }
        public int? SpellBlinkMaxDistance { get; }

        public bool IsEquippable => Category == ItemCategory.Equipment && EquipSlot != EquipmentSlot.None;
        // 統合後は「装備スロットがSpellbookかどうか」で魔法書判定します。
        public bool IsSpellbook => EquipSlot == EquipmentSlot.Spellbook;

        public ItemDefinition(
            ItemId itemType,
            string displayName,
            ItemCategory category,
            EquipmentSlot equipSlot,
            int attackBonus,
            int defenseBonus,
            bool consumesOnUse,
            string description = "",
            string shortSpellName = "",
            int? spellRange = null,
            int? spellBaseHungerCost = null,
            int? spellDamageMin = null,
            int? spellDamageMax = null,
            int? spellStatusTurnsMin = null,
            int? spellStatusTurnsMax = null,
            int? spellBlinkMinDistance = null,
            int? spellBlinkMaxDistance = null)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            // min/max の対になる値は「両方指定 or 両方null」を強制します。
            ValidateOptionalPair(spellDamageMin, spellDamageMax, nameof(spellDamageMin), nameof(spellDamageMax));
            ValidateOptionalPair(spellStatusTurnsMin, spellStatusTurnsMax, nameof(spellStatusTurnsMin), nameof(spellStatusTurnsMax));
            ValidateOptionalPair(spellBlinkMinDistance, spellBlinkMaxDistance, nameof(spellBlinkMinDistance), nameof(spellBlinkMaxDistance));

            if (spellRange.HasValue && spellRange.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spellRange), "Spell range must be zero or greater.");
            }

            if (spellBaseHungerCost.HasValue && spellBaseHungerCost.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spellBaseHungerCost), "Spell hunger cost must be zero or greater.");
            }

            ItemType = itemType;
            DisplayName = displayName;
            Description = description ?? string.Empty;
            ShortSpellName = shortSpellName ?? string.Empty;
            Category = category;
            EquipSlot = equipSlot;
            AttackBonus = attackBonus;
            DefenseBonus = defenseBonus;
            ConsumesOnUse = consumesOnUse;
            SpellRange = spellRange;
            SpellBaseHungerCost = spellBaseHungerCost;
            SpellDamageMin = spellDamageMin;
            SpellDamageMax = spellDamageMax;
            SpellStatusTurnsMin = spellStatusTurnsMin;
            SpellStatusTurnsMax = spellStatusTurnsMax;
            SpellBlinkMinDistance = spellBlinkMinDistance;
            SpellBlinkMaxDistance = spellBlinkMaxDistance;
        }

        public static ItemDefinition CreateUnknown(ItemId itemType)
        {
            return new ItemDefinition(
                itemType,
                itemType.ToString(),
                ItemCategory.Consumable,
                EquipmentSlot.None,
                0,
                0,
                true,
                description: "不明なアイテムです。");
        }

        private static void ValidateOptionalPair(int? min, int? max, string minName, string maxName)
        {
            if (min.HasValue != max.HasValue)
            {
                throw new ArgumentException($"{minName} and {maxName} must both be specified or both be null.");
            }

            if (!min.HasValue)
            {
                return;
            }

            if (min.Value < 0)
            {
                throw new ArgumentOutOfRangeException(minName, $"{minName} must be zero or greater.");
            }

            if (max.Value < 0)
            {
                throw new ArgumentOutOfRangeException(maxName, $"{maxName} must be zero or greater.");
            }

            if (min.Value > max.Value)
            {
                throw new ArgumentException($"{minName} must be less than or equal to {maxName}.");
            }
        }
    }
}


