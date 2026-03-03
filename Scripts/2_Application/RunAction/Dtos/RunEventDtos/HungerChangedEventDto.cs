using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct HungerChangedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.HungerChanged;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public float Delta { get; }

        public HungerChangedEventDto(int turnNumber, Guid actorId, float delta)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            Delta = delta;
        }
    }
}
