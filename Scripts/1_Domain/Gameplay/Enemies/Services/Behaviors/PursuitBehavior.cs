// =============================================================================
// PursuitBehavior.cs
// =============================================================================
// 概要:
//   敵の追跡行動を実装するクラス。プレイヤーに向かって移動します。
//
// 優先度: 40
//   - 攻撃行動（55-60）より低い優先度
//   - 徘徊（20）より高い優先度
//   - 逃走（85）より低い優先度
//
// 実行条件:
//   - AI状態がPursuingまたはAttacking
//   - プレイヤーが見えているか、最後に見た位置を記憶している
//
// 追跡ロジック:
//   - プレイヤーが見えている場合: プレイヤー位置に向かって移動
//   - プレイヤーを見失った場合: 最後に見た位置（LastKnownPlayerPosition）へ移動
//   - IPathfindingService.GetNextStep()を使用して次の一歩を計算
//   - PreferredDistanceを考慮（遠距離タイプは適正距離を維持）
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
    /// 追跡行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// プレイヤー（または最後に見た位置）に向かって移動します。
    /// 優先度40で、攻撃できない状況での主要な行動です。
    /// </remarks>
    public class PursuitBehavior : IEnemyBehavior
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
        /// PursuitBehaviorを作成します。
        /// </summary>
        public PursuitBehavior(IPathfindingService pathfindingService = null)
        {
            _pathfindingService = pathfindingService;
        }

        /// <inheritdoc/>
        public int Priority => 40;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 追跡状態または帰還状態で、ターゲット位置がある場合
            if (memory.CurrentState != AiState.Pursuing && memory.CurrentState != AiState.Returning)
                return false;

            return memory.LastKnownPlayerPosition.HasValue;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var target = memory.LastKnownPlayerPosition.Value;
            var targetActor = session.GetActorAt(target);
            // 追跡対象が敵対Actorであれば、その占有セルをゴールとしてのみ許可する。
            // これにより「ゴール占有だから探索失敗」という追跡不能を防ぐ。
            var canEnterTargetCell = targetActor != null && targetActor.Faction != enemy.Faction;
            // 占有情報は毎ターンのセッション状態から構築し、探索ルール自体はサービス側に集約する。
            var occupiedPositions = CollectOccupiedPositions(session, enemy);

            // 知性に応じてパス探索を使用
            Position? nextPos = null;

            if (_pathfindingService != null && profile.Intelligence >= IntelligenceLevel.Medium)
            {
                nextPos = _pathfindingService.GetNextStep(
                    session.Map,
                    enemy.Position,
                    target,
                    occupiedPositions,
                    canEnterTargetCell);
            }

            // パス探索が使えない場合は貪欲法
            if (!nextPos.HasValue)
            {
                nextPos = GetGreedyStep(enemy, session, target);
            }

            if (!nextPos.HasValue)
            {
                return new WaitAction(enemy.Id);
            }

            var direction = GetDirection(enemy.Position, nextPos.Value);
            return new MoveAction(enemy.Id, direction);
        }

        private Position? GetGreedyStep(Actor enemy, RunSession session, Position target)
        {
            Position? bestPos = null;
            int bestDistance = int.MaxValue;

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

                var distance = ChebyshevDistance(next, target);
                if (distance < bestDistance)
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

            // 自分以外の生存Actorだけを占有として扱う（selfは除外）。
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

                // 同陣営/敵対を問わず「現在立っているセル」は通行不可として収集する。
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




