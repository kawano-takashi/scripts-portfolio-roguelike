using System.Collections.Generic;
using System.Linq;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.Inventory.Types;

namespace Roguelike.Presentation.Gameplay.Inventory.Formatting
{
    /// <summary>
    /// インベントリ表示文言を生成します。
    /// </summary>
    public sealed class InventoryFormatter
    {
        public string GetPrimaryActionLabel()
        {
            return "Use";
        }

        public string GetEquipActionLabel(bool isEquippable, bool isEquipped)
        {
            if (!isEquippable)
            {
                return "Equip";
            }

            return isEquipped ? "Unequip" : "Equip";
        }

        public string FormatItemDisplayName(InventoryItemDto item)
        {
            if (item.ItemId == System.Guid.Empty)
            {
                return "Unknown Item";
            }

            var name = BuildDisplayName(item.BaseDisplayName, item.EnhancementLevels);
            if (item.IsEquipped)
            {
                return $"{name} [装備]";
            }

            return name;
        }

        public string FormatDetailedDescription(InventoryItemDto item)
        {
            if (item.ItemId == System.Guid.Empty)
            {
                return "不明なアイテムです。";
            }

            var baseDescription = string.IsNullOrWhiteSpace(item.BaseDescription)
                ? "不明なアイテムです。"
                : item.BaseDescription;
            return BuildDetailedDescription(baseDescription, item.EnhancementLevels);
        }

        public string GetDetailMenuLabel(
            ItemDetailMenuOption option,
            bool isEquippable,
            bool isEquipped)
        {
            return option switch
            {
                ItemDetailMenuOption.Use => GetPrimaryActionLabel(),
                ItemDetailMenuOption.Equip => GetEquipActionLabel(isEquippable, isEquipped),
                ItemDetailMenuOption.Drop => "置く",
                ItemDetailMenuOption.SpellPreview => "プレビュー",
                ItemDetailMenuOption.Description => "説明",
                _ => option.ToString()
            };
        }

        private static string BuildDisplayName(
            string baseDisplayName,
            IReadOnlyList<InventoryItemDto.EnhancementLevelDto> enhancements)
        {
            var normalizedBaseName = string.IsNullOrWhiteSpace(baseDisplayName) ? "Unknown Item" : baseDisplayName;
            if (enhancements == null || enhancements.Count == 0)
            {
                return normalizedBaseName;
            }

            var suffix = string.Join("/", enhancements.Select(level =>
                $"{GetCategoryShortName(level.Category)}+{level.Level}"));
            return $"{normalizedBaseName} [{suffix}]";
        }

        private static string BuildDetailedDescription(
            string baseDescription,
            IReadOnlyList<InventoryItemDto.EnhancementLevelDto> enhancements)
        {
            if (enhancements == null || enhancements.Count == 0)
            {
                return baseDescription;
            }

            var lines = string.Join("\n", enhancements.Select(level =>
                $"{GetCategoryDisplayName(level.Category)} Lv.{level.Level}"));
            return $"{baseDescription}\n\n--- 強化 ---\n{lines}";
        }

        private static string GetCategoryShortName(SpellEnhancementCategoryDto category)
        {
            return category switch
            {
                SpellEnhancementCategoryDto.Power => "POW",
                SpellEnhancementCategoryDto.Range => "RNG",
                SpellEnhancementCategoryDto.Efficiency => "EFF",
                _ => category.ToString()
            };
        }

        private static string GetCategoryDisplayName(SpellEnhancementCategoryDto category)
        {
            return category switch
            {
                SpellEnhancementCategoryDto.Power => "威力強化",
                SpellEnhancementCategoryDto.Range => "射程強化",
                SpellEnhancementCategoryDto.Efficiency => "コスト軽減",
                _ => category.ToString()
            };
        }
    }
}




