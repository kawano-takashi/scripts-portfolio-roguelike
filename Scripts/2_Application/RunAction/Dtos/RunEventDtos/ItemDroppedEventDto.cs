using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ItemDroppedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ItemDropped;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public Guid ItemId { get; }
        public int ItemTypeValue { get; }
        public GridPositionDto DropPosition { get; }

        public ItemDroppedEventDto(
            int turnNumber,
            Guid actorId,
            Guid itemId,
            int itemTypeValue,
            GridPositionDto dropPosition)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
            DropPosition = dropPosition;
        }
    }
}
