using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ItemUsedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ItemUsed;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public Guid ItemId { get; }
        public int ItemTypeValue { get; }

        public ItemUsedEventDto(int turnNumber, Guid actorId, Guid itemId, int itemTypeValue)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
        }
    }
}
