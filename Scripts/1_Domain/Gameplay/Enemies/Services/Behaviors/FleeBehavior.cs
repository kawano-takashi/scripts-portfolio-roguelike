// =============================================================================
// FleeBehavior.cs
// =============================================================================
// 概要:
//   敵の逃走行動を実装するクラス。HP閾値以下になるとプレイヤーから逃げます。
//
// 優先度: 85（最高優先度）
//   - 生存を最優先とするため、攻撃行動より高い優先度を持つ
//   - HPが危険域に達したら、他の行動より優先して逃走を選択
//
// 実行条件:
//   - AI状態がFleeing
//   - HPがFleeHpThresholdPercent以下
//
// 逃走ロジック:
//   - IPathfindingService.GetFleeStep()を使用してプレイヤーから最も遠ざかる方向へ移動
//   - 移動できない場合は待機
// =============================================================================

using System;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// 逃走行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// HP閾値以下になるとプレイヤーから逃げます。
    /// 優先度85で最も高く、生存を最優先します。
    /// </remarks>
    public class FleeBehavior : IEnemyBehavior
    {
        private readonly IPathfindingService _pathfindingService;

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

        /// <summary>
        /// FleeBehaviorを作成します。
        /// </summary>
        public FleeBehavior(IPathfindingService pathfindingService = null)
        {
            _pathfindingService = pathfindingService;
        }

        /// <inheritdoc/>
        public int Priority => 85;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 逃走状態のとき
            return memory.CurrentState == AiState.Fleeing;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var player = session.Player;
            if (player == null || player.IsDead)
            {
                return new WaitAction(enemy.Id);
            }

            // プレイヤーから離れる
            Position? nextPos = null;
            // 逃走時も毎ターンの占有情報を明示的に渡し、探索判定をサービス側に一元化する。
            var occupiedPositions = CollectOccupiedPositions(session, enemy);

            if (_pathfindingService != null)
            {
                nextPos = _pathfindingService.GetFleeStep(
                    session.Map,
                    enemy.Position,
                    player.Position,
                    occupiedPositions);
            }

            // フォールバック: 貪欲法で逃げる
            if (!nextPos.HasValue)
            {
                nextPos = GetFleeStep(enemy, session, player.Position);
            }

            if (!nextPos.HasValue)
            {
                return new WaitAction(enemy.Id);
            }

            var direction = GetDirection(enemy.Position, nextPos.Value);
            return new MoveAction(enemy.Id, direction);
        }

        private Position? GetFleeStep(Actor enemy, RunSession session, Position threat)
        {
            Position? bestPos = null;
            int bestDistance = -1;

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

                // 脅威から最も遠い位置を選ぶ
                var distance = ChebyshevDistance(next, threat);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPos = next;
                }
            }

            return bestPos;
        }

        private static ISet<Position> CollectOccupiedPositions(RunSession session, Actor self)
        {
            var occupied = new HashSet<Position>();

            // 自分以外の生存Actorを占有として扱う。
            if (session?.Player != null && session.Player != self && !session.Player.IsDead)
            {
                occupied.Add(session.Player.Position);
            }

            if (session?.Enemies == null)
            {
                return occupied;
            }

            for (int i = 0; i < session.Enemies.Count; i++)
            {
                var enemy = session.Enemies[i];
                if (enemy == null || enemy == self || enemy.IsDead)
                {
                    continue;
                }

                // 逃走先の衝突回避のため、敵味方問わず占有セルを収集する。
                occupied.Add(enemy.Position);
            }

            return occupied;
        }

        private static Direction GetDirection(Position from, Position to)
        {
            int dx = Math.Sign(to.X - from.X);
            int dy = Math.Sign(to.Y - from.Y);

            return (dx, dy) switch
            {
                (0, -1) => Direction.Up,
                (1, -1) => Direction.UpRight,
                (1, 0) => Direction.Right,
                (1, 1) => Direction.DownRight,
                (0, 1) => Direction.Down,
                (-1, 1) => Direction.DownLeft,
                (-1, 0) => Direction.Left,
                (-1, -1) => Direction.UpLeft,
                _ => Direction.Down // fallback
            };
        }

        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}




