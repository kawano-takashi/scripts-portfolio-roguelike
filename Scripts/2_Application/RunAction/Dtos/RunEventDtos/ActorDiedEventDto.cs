using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ActorDiedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ActorDied;
        public int TurnNumber { get; }
        public Guid ActorId { get; }

        public ActorDiedEventDto(int turnNumber, Guid actorId)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
        }
    }
}
