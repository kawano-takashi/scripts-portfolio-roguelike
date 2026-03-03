// =============================================================================
// IEnemyBehavior.cs
// =============================================================================
// 概要:
//   敵AIの行動を定義するStrategyパターンのインターフェース。
//   各行動（攻撃、追跡、逃走、徘徊等）はこのインターフェースを実装します。
//
// 設計パターン:
//   - Strategy Pattern: 行動ごとにクラスを分離し、実行時に切り替え可能
//   - Priority-based Selection: 優先度順に評価し、最初に実行可能なものを選択
//
// 実装クラス（優先度順）:
//   1. FleeBehavior (Priority: 85) - HP低下時の逃走
//   2. MeleeAttackBehavior (Priority: 60) - 近接攻撃
//   3. RangedAttackBehavior (Priority: 55) - 遠距離攻撃
//   4. PursuitBehavior (Priority: 40) - プレイヤー追跡
//   5. WanderBehavior (Priority: 20) - ランダム徘徊
//   6. SleepBehavior (Priority: 10) - 睡眠状態
//   7. WaitBehavior (Priority: 0) - 何もしない（フォールバック）
//
// 使用方法:
//   EnemyAiServiceが優先度順に各行動のCanExecute()を評価し、
//   trueを返した最初の行動のExecute()を呼び出してRoguelikeActionを取得します。
// =============================================================================

using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// 敵の行動を定義するインターフェースです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Strategy Patternを使用して、様々な行動を実装します。
    /// 各行動には優先度があり、複数の行動が実行可能な場合は
    /// 最も優先度の高いものが選択されます。
    /// </para>
    /// </remarks>
    public interface IEnemyBehavior
    {
        /// <summary>
        /// この行動の優先度。数値が大きいほど優先されます。
        /// 複数の行動が実行可能な場合、最も優先度の高いものが選ばれます。
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// この行動が実行可能かを判定します。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="session">ランセッション</param>
        /// <param name="memory">敵のAI記憶</param>
        /// <param name="profile">敵のプロファイル</param>
        /// <returns>実行可能ならtrue</returns>
        bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile);

        /// <summary>
        /// この行動を実行し、結果のアクションを返します。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="session">ランセッション</param>
        /// <param name="memory">敵のAI記憶</param>
        /// <param name="profile">敵のプロファイル</param>
        /// <returns>実行するアクション</returns>
        RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile);
    }
}




