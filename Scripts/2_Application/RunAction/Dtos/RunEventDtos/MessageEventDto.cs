using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct MessageEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.Message;
        public int TurnNumber { get; }
        public string Message { get; }

        public MessageEventDto(int turnNumber, string message)
        {
            TurnNumber = turnNumber;
            Message = message;
        }
    }
}
