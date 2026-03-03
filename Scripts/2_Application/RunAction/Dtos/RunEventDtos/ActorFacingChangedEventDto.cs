using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ActorFacingChangedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ActorFacingChanged;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public int DirectionValue { get; }

        public ActorFacingChangedEventDto(int turnNumber, Guid actorId, int directionValue)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            DirectionValue = directionValue;
        }
    }
}
