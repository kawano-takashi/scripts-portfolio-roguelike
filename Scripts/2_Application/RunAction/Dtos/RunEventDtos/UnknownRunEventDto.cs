using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 未対応イベントが投影されたときに返すフォールバックDTOです。
    /// </summary>
    public readonly struct UnknownRunEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.Unknown;
        public int TurnNumber { get; }
        public string SourceEventTypeName { get; }

        public UnknownRunEventDto(int turnNumber, string sourceEventTypeName)
        {
            TurnNumber = turnNumber;
            SourceEventTypeName = sourceEventTypeName;
        }
    }
}
