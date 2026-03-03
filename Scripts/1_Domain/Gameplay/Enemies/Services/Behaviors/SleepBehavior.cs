// =============================================================================
// SleepBehavior.cs
// =============================================================================
// 概要:
//   敵の睡眠行動を実装するクラス。睡眠状態の敵は何もしません。
//
// 優先度: 10
//   - 待機（0）より高い優先度
//   - 徘徊（20）より低い優先度
//   - 睡眠状態専用の行動
//
// 実行条件:
//   - AI状態がSleeping
//
// 睡眠ロジック:
//   - 常にWaitActionを返す（何もしない）
//   - 睡眠からの覚醒はAdvancedEnemyAiService.UpdateMemory()で処理
//   - 覚醒条件: プレイヤーが視界内に入る、または攻撃を受ける
//
// 設計意図:
//   - 睡眠状態の敵はダンジョン内の安全地帯を提供
//   - プレイヤーは睡眠中の敵を避けて進むか、先制攻撃するかを選択可能
// =============================================================================

using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// 睡眠行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 睡眠状態の敵は何もしません。
    /// プレイヤーが接近するか攻撃を受けると起きます。
    /// </para>
    /// <para>
    /// 優先度10で、待機より高く徘徊より低い優先度です。
    /// </para>
    /// </remarks>
    public class SleepBehavior : IEnemyBehavior
    {
        /// <inheritdoc/>
        public int Priority => 10;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 睡眠状態のときのみ
            return memory.CurrentState == AiState.Sleeping;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 睡眠中は何もしない
            return new WaitAction(enemy.Id);
        }
    }
}




