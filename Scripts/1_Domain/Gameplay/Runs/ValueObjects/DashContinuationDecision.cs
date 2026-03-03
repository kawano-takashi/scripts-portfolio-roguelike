using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.ValueObjects
{
    /// <summary>
    /// ダッシュ継続可否の判定結果です。
    /// </summary>
    public readonly struct DashContinuationDecision
    {
        public bool ShouldContinue { get; }
        public Direction NextDirection { get; }
        public DashContinuationStopReason StopReason { get; }

        private DashContinuationDecision(
            bool shouldContinue,
            Direction nextDirection,
            DashContinuationStopReason stopReason)
        {
            ShouldContinue = shouldContinue;
            NextDirection = nextDirection;
            StopReason = stopReason;
        }

        public static DashContinuationDecision Continue(Direction nextDirection)
        {
            return new DashContinuationDecision(true, nextDirection, DashContinuationStopReason.None);
        }

        public static DashContinuationDecision Stop(DashContinuationStopReason stopReason, Direction nextDirection)
        {
            return new DashContinuationDecision(false, nextDirection, stopReason);
        }
    }
}


