using Roguelike.Application.Enums;

namespace Roguelike.Application.ValueObjects
{
    /// <summary>
    /// ダッシュ1ステップ後の継続可否です。
    /// </summary>
    public readonly struct DashStepDecision
    {
        /// <summary>
        /// 次のステップへ継続するか。
        /// </summary>
        public bool ShouldContinue { get; }
        /// <summary>
        /// 継続時に採用する次方向。
        /// 停止時は入力方向維持など、呼び出し側が参照しやすい値を保持します。
        /// </summary>
        public int NextDirectionValue { get; }
        /// <summary>
        /// 停止時の理由。
        /// 継続時は <see cref="DashStopReason.None"/>。
        /// </summary>
        public DashStopReason StopReason { get; }

        private DashStepDecision(bool shouldContinue, int nextDirectionValue, DashStopReason stopReason)
        {
            ShouldContinue = shouldContinue;
            NextDirectionValue = nextDirectionValue;
            StopReason = stopReason;
        }

        /// <summary>
        /// 継続判定の結果を作成します。
        /// </summary>
        public static DashStepDecision Continue(int nextDirectionValue)
        {
            return new DashStepDecision(true, nextDirectionValue, DashStopReason.None);
        }

        /// <summary>
        /// 停止判定の結果を作成します。
        /// </summary>
        public static DashStepDecision Stop(DashStopReason stopReason, int nextDirectionValue)
        {
            return new DashStepDecision(false, nextDirectionValue, stopReason);
        }
    }
}
