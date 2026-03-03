using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ActorMovedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ActorMoved;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public bool Success { get; }
        public GridPositionDto FromPosition { get; }
        public GridPositionDto ToPosition { get; }

        public ActorMovedEventDto(
            int turnNumber,
            Guid actorId,
            bool success,
            GridPositionDto fromPosition,
            GridPositionDto toPosition)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            Success = success;
            FromPosition = fromPosition;
            ToPosition = toPosition;
        }
    }
}
