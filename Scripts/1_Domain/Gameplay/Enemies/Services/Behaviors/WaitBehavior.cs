// =============================================================================
// WaitBehavior.cs
// =============================================================================
// 概要:
//   敵の待機行動を実装するクラス。何もできないときのフォールバックとして使用します。
//
// 優先度: 0（最低優先度）
//   - すべての行動より低い優先度
//   - 他のどの行動も実行できないときに選択される
//
// 実行条件:
//   - 常に実行可能（CanExecuteは常にtrueを返す）
//
// 設計意図:
//   - Null Objectパターンの適用
//   - 行動選択アルゴリズムが必ず有効な行動を返せるようにする
//   - BehaviorSelectorが空の結果を返すことを防止
//
// 使用場面:
//   - 移動も攻撃もできない状況（周囲が埋まっている等）
//   - 未知のAI状態が発生した場合のフォールバック
// =============================================================================

using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// 待機行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 何もできないときのフォールバックとして使用します。
    /// 優先度0は最も低く、他のどの行動も実行できないときに選択されます。
    /// </para>
    /// <para>
    /// Null Objectパターンを適用しており、行動選択が必ず有効な結果を
    /// 返せるようにしています。
    /// </para>
    /// </remarks>
    public class WaitBehavior : IEnemyBehavior
    {
        /// <inheritdoc/>
        public int Priority => 0;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 常に実行可能（フォールバック）
            return true;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            return new WaitAction(enemy.Id);
        }
    }
}




