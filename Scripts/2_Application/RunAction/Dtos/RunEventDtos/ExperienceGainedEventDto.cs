using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ExperienceGainedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ExperienceGained;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public Guid? SourceEnemyId { get; }
        public int Amount { get; }
        public int CurrentExp { get; }
        public int ExpToNextLevel { get; }

        public ExperienceGainedEventDto(
            int turnNumber,
            Guid actorId,
            Guid? sourceEnemyId,
            int amount,
            int currentExp,
            int expToNextLevel)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            SourceEnemyId = sourceEnemyId;
            Amount = amount;
            CurrentExp = currentExp;
            ExpToNextLevel = expToNextLevel;
        }
    }
}
