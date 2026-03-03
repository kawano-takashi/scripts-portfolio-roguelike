using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ActorDamagedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ActorDamaged;
        public int TurnNumber { get; }
        public Guid? SourceActorId { get; }
        public Guid TargetActorId { get; }
        public int Amount { get; }
        public int RemainingHp { get; }

        public ActorDamagedEventDto(
            int turnNumber,
            Guid? sourceActorId,
            Guid targetActorId,
            int amount,
            int remainingHp)
        {
            TurnNumber = turnNumber;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            Amount = amount;
            RemainingHp = remainingHp;
        }
    }
}
