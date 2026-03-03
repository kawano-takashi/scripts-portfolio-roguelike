using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ItemPickedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ItemPicked;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public int ItemTypeValue { get; }
        public GridPositionDto Position { get; }

        public ItemPickedEventDto(int turnNumber, Guid actorId, int itemTypeValue, GridPositionDto position)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            ItemTypeValue = itemTypeValue;
            Position = position;
        }
    }
}
