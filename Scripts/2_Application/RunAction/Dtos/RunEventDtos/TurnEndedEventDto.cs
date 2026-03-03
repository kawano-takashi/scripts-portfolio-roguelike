using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct TurnEndedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.TurnEnded;
        public int TurnNumber { get; }

        public TurnEndedEventDto(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
}
