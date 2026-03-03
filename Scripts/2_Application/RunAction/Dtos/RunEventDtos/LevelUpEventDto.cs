using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct LevelUpEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.LevelUp;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public int OldLevel { get; }
        public int NewLevel { get; }

        public LevelUpEventDto(int turnNumber, Guid actorId, int oldLevel, int newLevel)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }
}
