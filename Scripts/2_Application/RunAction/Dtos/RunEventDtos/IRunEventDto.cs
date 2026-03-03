using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// ターン内イベントDTOの共通契約です。
    /// </summary>
    public interface IRunEventDto
    {
        RunEventKind Kind { get; }
        int TurnNumber { get; }
    }
}
