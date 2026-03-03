using System;
using System.Collections.Generic;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// UI向けのインベントリアイテム投影です。
    /// </summary>
    public readonly struct InventoryItemDto
    {
        private static readonly IReadOnlyList<EnhancementLevelDto> EmptyEnhancements =
            Array.Empty<EnhancementLevelDto>();

        public Guid ItemId { get; }
        public int ItemTypeValue { get; }
        public ItemTypeDto ItemType => (ItemTypeDto)ItemTypeValue;
        public string BaseDisplayName { get; }
        public string BaseDescription { get; }
        public IReadOnlyList<EnhancementLevelDto> EnhancementLevels { get; }
        public bool IsEquippable { get; }
        public bool IsEquipped { get; }
        public bool CanUse { get; }
        public bool CanDrop { get; }
        public bool CanToggleEquip { get; }
        public bool IsSpellbook { get; }
        public bool CanShowSpellPreview { get; }

        public InventoryItemDto(
            Guid itemId,
            int itemTypeValue,
            string baseDisplayName,
            string baseDescription,
            IReadOnlyList<EnhancementLevelDto> enhancementLevels,
            bool isEquippable,
            bool isEquipped,
            bool canUse,
            bool canDrop,
            bool canToggleEquip,
            bool isSpellbook,
            bool canShowSpellPreview)
        {
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
            BaseDisplayName = baseDisplayName;
            BaseDescription = baseDescription;
            EnhancementLevels = enhancementLevels ?? EmptyEnhancements;
            IsEquippable = isEquippable;
            IsEquipped = isEquipped;
            CanUse = canUse;
            CanDrop = canDrop;
            CanToggleEquip = canToggleEquip;
            IsSpellbook = isSpellbook;
            CanShowSpellPreview = canShowSpellPreview;
        }

        public readonly struct EnhancementLevelDto
        {
            public int CategoryValue { get; }
            public SpellEnhancementCategoryDto Category => (SpellEnhancementCategoryDto)CategoryValue;
            public int Level { get; }

            public EnhancementLevelDto(int categoryValue, int level)
            {
                CategoryValue = categoryValue;
                Level = level;
            }
        }
    }
}
