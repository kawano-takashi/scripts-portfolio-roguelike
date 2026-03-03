using System;
using System.Collections.Generic;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Types
{
    /// <summary>
    /// 1ターン分の演出再生計画です。
    /// </summary>
    public sealed class TurnPresentationPlan
    {
        /// <summary>
        /// 対象ターン番号です。
        /// </summary>
        public int TurnNumber { get; }

        /// <summary>
        /// 再生ステップ一覧です。
        /// </summary>
        public IReadOnlyList<TurnPresentationStep> Steps { get; }

        /// <summary>
        /// 再生計画を作成します。
        /// </summary>
        /// <param name="turnNumber">対応するターン番号。</param>
        /// <param name="steps">再生ステップ列。null の場合は空配列として扱います。</param>
        public TurnPresentationPlan(int turnNumber, IReadOnlyList<TurnPresentationStep> steps)
        {
            TurnNumber = turnNumber;
            // 呼び出し側で null を渡しても Presentation で安全に列挙できるように空配列へ正規化します。
            Steps = steps ?? Array.Empty<TurnPresentationStep>();
        }
    }
}



