using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// ダッシュ1ステップの解決結果DTOです。
    /// </summary>
    public readonly struct DashStepResultDto
    {
        public RunTurnResultDto Resolution { get; }
        public bool ShouldContinue { get; }
        public int NextDirectionValue { get; }
        public DashStopReason StopReason { get; }

        public DashStepResultDto(
            RunTurnResultDto resolution,
            bool shouldContinue,
            int nextDirectionValue,
            DashStopReason stopReason)
        {
            Resolution = resolution;
            ShouldContinue = shouldContinue;
            NextDirectionValue = nextDirectionValue;
            StopReason = stopReason;
        }
    }
}
