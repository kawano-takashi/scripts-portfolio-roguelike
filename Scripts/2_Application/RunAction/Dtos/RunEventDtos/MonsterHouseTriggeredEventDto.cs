using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct MonsterHouseTriggeredEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.MonsterHouseTriggered;
        public int TurnNumber { get; }
        public int AwakenedEnemyCount { get; }

        public MonsterHouseTriggeredEventDto(int turnNumber, int awakenedEnemyCount)
        {
            TurnNumber = turnNumber;
            AwakenedEnemyCount = awakenedEnemyCount;
        }
    }
}
