using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// インベントリ関連のReadModelを組み立てるクエリサービスです。
    /// </summary>
    public sealed class InventoryReadModelService
    {
        private static readonly IReadOnlyList<InventoryItemDto> EmptyInventoryItems = Array.Empty<InventoryItemDto>();

        private readonly IRunReadStore _runRepository;

        public InventoryReadModelService(IRunReadStore runRepository)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
        }

        public bool TryGetInventoryItems(out IReadOnlyList<InventoryItemDto> items)
        {
            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Player?.Inventory == null)
            {
                items = EmptyInventoryItems;
                return false;
            }

            var sourceItems = run.Player.Inventory.Items;
            if (sourceItems == null || sourceItems.Count == 0)
            {
                items = EmptyInventoryItems;
                return true;
            }

            var projected = new List<InventoryItemDto>(sourceItems.Count);
            for (var i = 0; i < sourceItems.Count; i++)
            {
                var item = sourceItems[i];
                if (item == null)
                {
                    continue;
                }

                projected.Add(ToInventoryItemDto(run.Player, item));
            }

            items = projected;
            return true;
        }

        public bool TryGetInventoryItem(Guid itemId, out InventoryItemDto item)
        {
            if (itemId == Guid.Empty)
            {
                item = default;
                return false;
            }

            var itemInstanceId = new ItemInstanceId(itemId);
            if (_runRepository.TryGetCurrent(out var run) &&
                TryResolvePlayerInventoryItem(run?.Player, itemInstanceId, out var resolved))
            {
                item = ToInventoryItemDto(run.Player, resolved);
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetSpellPreviewContext(Guid itemId, out SpellPreviewContext context)
        {
            context = default;
            if (itemId == Guid.Empty)
            {
                return false;
            }

            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Player == null ||
                run.Map == null)
            {
                return false;
            }

            var itemInstanceId = new ItemInstanceId(itemId);
            if (!TryResolvePlayerInventoryItem(run.Player, itemInstanceId, out var item) ||
                !TryResolveSpellRange(item, out var range))
            {
                return false;
            }

            context = new SpellPreviewContext(
                run.Map,
                run.Player.Position,
                run.Player.Facing,
                range);
            return true;
        }

        private static bool TryResolvePlayerInventoryItem(Actor player, ItemInstanceId itemId, out InventoryItem item)
        {
            item = null;
            var inventory = player?.Inventory;
            return inventory != null && inventory.TryGetById(itemId, out item);
        }

        private static InventoryItemDto ToInventoryItemDto(Actor player, InventoryItem item)
        {
            var definition = ItemCatalog.GetDefinition(item.ItemType);
            var canShowSpellPreview = TryResolveSpellRange(item, out _);
            var enhancementLevels = BuildEnhancementLevels(item);

            return new InventoryItemDto(
                itemId: item.Id.Value,
                itemTypeValue: (int)item.ItemType,
                baseDisplayName: definition.DisplayName,
                baseDescription: definition.Description,
                enhancementLevels: enhancementLevels,
                isEquippable: definition.IsEquippable,
                isEquipped: player?.Equipment?.IsEquipped(item.Id) == true,
                canUse: true,
                canDrop: true,
                canToggleEquip: definition.IsEquippable,
                isSpellbook: definition.IsSpellbook,
                canShowSpellPreview: canShowSpellPreview);
        }

        private static IReadOnlyList<InventoryItemDto.EnhancementLevelDto> BuildEnhancementLevels(InventoryItem item)
        {
            if (item?.Enhancements == null || !item.Enhancements.HasAny)
            {
                return Array.Empty<InventoryItemDto.EnhancementLevelDto>();
            }

            var levels = new List<InventoryItemDto.EnhancementLevelDto>(item.Enhancements.Levels.Count);
            foreach (var level in item.Enhancements.Levels)
            {
                levels.Add(new InventoryItemDto.EnhancementLevelDto((int)level.Key, level.Value));
            }

            return levels;
        }

        private static bool TryResolveSpellRange(InventoryItem item, out int range)
        {
            range = 0;
            if (item == null ||
                !ItemCatalog.TryGetSpellDefinition(item.ItemType, out var spellDefinition))
            {
                return false;
            }

            range = spellDefinition.SpellRange.GetValueOrDefault();
            return range >= 1;
        }

        public readonly struct SpellPreviewContext
        {
            public Map Map { get; }
            public Position Origin { get; }
            public Direction Facing { get; }
            public int Range { get; }

            public SpellPreviewContext(Map map, Position origin, Direction facing, int range)
            {
                Map = map;
                Origin = origin;
                Facing = facing;
                Range = range;
            }
        }
    }
}

