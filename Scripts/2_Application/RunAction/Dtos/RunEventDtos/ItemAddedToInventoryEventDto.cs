using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ItemAddedToInventoryEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ItemAddedToInventory;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public Guid ItemId { get; }
        public int ItemTypeValue { get; }
        public GridPositionDto PickupPosition { get; }

        public ItemAddedToInventoryEventDto(
            int turnNumber,
            Guid actorId,
            Guid itemId,
            int itemTypeValue,
            GridPositionDto pickupPosition)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
            PickupPosition = pickupPosition;
        }
    }
}
