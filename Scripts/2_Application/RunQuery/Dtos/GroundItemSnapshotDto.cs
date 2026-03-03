using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 床置きアイテム表示用スナップショットDTOです。
    /// </summary>
    public readonly struct GroundItemSnapshotDto
    {
        public Guid ItemId { get; }
        public int ItemTypeValue { get; }
        public ItemTypeDto ItemType => (ItemTypeDto)ItemTypeValue;
        public bool IsSpellbook => ItemType.IsSpellbook();
        public GridPositionDto Position { get; }

        public GroundItemSnapshotDto(Guid itemId, int itemTypeValue, GridPositionDto position)
        {
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
            Position = position;
        }
    }
}
