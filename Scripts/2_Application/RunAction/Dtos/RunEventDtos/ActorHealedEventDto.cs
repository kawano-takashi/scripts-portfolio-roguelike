using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ActorHealedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ActorHealed;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public int Amount { get; }
        public int CurrentHp { get; }

        public ActorHealedEventDto(int turnNumber, Guid actorId, int amount, int currentHp)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            Amount = amount;
            CurrentHp = currentHp;
        }
    }
}
