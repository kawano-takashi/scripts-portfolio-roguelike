using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// ラン終了系イベントのDTOです。
    /// </summary>
    public readonly struct RunLifecycleEventDto
    {
        public RunLifecycleEventKind Kind { get; }
        public int Floor { get; }
        public int TotalTurns { get; }
        public int PlayerLevel { get; }
        public string SourceEventTypeName { get; }

        public bool IsVictory => Kind == RunLifecycleEventKind.RunCleared;

        public RunLifecycleEventDto(
            RunLifecycleEventKind kind,
            int floor,
            int totalTurns,
            int playerLevel,
            string sourceEventTypeName = null)
        {
            Kind = kind;
            Floor = floor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
            SourceEventTypeName = sourceEventTypeName;
        }
    }
}
