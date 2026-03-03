// =============================================================================
// WanderBehavior.cs
// =============================================================================
// 概要:
//   敵の徘徊行動を実装するクラス。プレイヤーを認識していないときにランダムに移動します。
//
// 優先度: 20
//   - 睡眠（10）より高い優先度
//   - 追跡（40）より低い優先度
//   - 攻撃行動（55-60）より低い優先度
//
// 実行条件:
//   - AI状態がWandering
//   - プレイヤーを視認しておらず、最後に見た位置も記憶していない
//
// 徘徊ロジック:
//   - 8方向から移動可能な方向をすべて収集
//   - RunSession.Randomを使用してランダムに1方向を選択
//   - 移動可能な方向がない場合は待機
//   - 斜め移動時は角抜けチェックを実施
// =============================================================================

using System;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// 徘徊行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// プレイヤーを認識していないときにランダムに移動します。
    /// 優先度20で、睡眠より高く追跡より低い優先度です。
    /// </remarks>
    public class WanderBehavior : IEnemyBehavior
    {
        // 8方向の移動オフセット
        private static readonly (int dx, int dy, Direction dir)[] Directions =
        {
            (0, -1, Direction.Up),
            (1, -1, Direction.UpRight),
            (1, 0, Direction.Right),
            (1, 1, Direction.DownRight),
            (0, 1, Direction.Down),
            (-1, 1, Direction.DownLeft),
            (-1, 0, Direction.Left),
            (-1, -1, Direction.UpLeft)
        };

        /// <inheritdoc/>
        public int Priority => 20;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 徘徊状態のときのみ
            return memory.CurrentState == AiState.Wandering;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var candidates = CollectMoveCandidates(enemy, session);

            if (candidates.Count == 0)
            {
                return new WaitAction(enemy.Id);
            }

            // ランダムに1つ選ぶ
            var index = session.Random.Next(candidates.Count);
            return new MoveAction(enemy.Id, candidates[index]);
        }

        private List<Direction> CollectMoveCandidates(Actor enemy, RunSession session)
        {
            var candidates = new List<Direction>(8);

            foreach (var (dx, dy, dir) in Directions)
            {
                var next = new Position(enemy.Position.X + dx, enemy.Position.Y + dy);

                if (!session.Map.Contains(next))
                    continue;

                if (!session.Map.IsWalkable(next))
                    continue;

                // 斜め移動の角抜けチェック
                if (dx != 0 && dy != 0)
                {
                    var horizontal = new Position(enemy.Position.X + dx, enemy.Position.Y);
                    var vertical = new Position(enemy.Position.X, enemy.Position.Y + dy);
                    if (!session.Map.IsWalkable(horizontal) || !session.Map.IsWalkable(vertical))
                        continue;
                }

                if (session.IsOccupied(next))
                    continue;

                candidates.Add(dir);
            }

            return candidates;
        }
    }
}




