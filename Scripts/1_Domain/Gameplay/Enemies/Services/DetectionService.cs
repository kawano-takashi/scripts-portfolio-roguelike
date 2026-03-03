// =============================================================================
// DetectionService.cs
// =============================================================================
// 概要:
//   IDetectionServiceの実装クラス。敵AIがプレイヤーを検知するための
//   各種判定ロジックを提供します。
//
// 依存サービス:
//   - IFieldOfViewService (MapContext): 視界計算に使用
//   - IPathfindingService (EnemyAIContext): 視線チェック（HasLineOfSight）に使用
//
// 距離計算:
//   チェビシェフ距離（8方向移動の最短距離）を使用。
//   斜め移動を1ターンと数えるローグライクの標準的な距離計算方式です。
// =============================================================================

using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// 敵のプレイヤー検知サービスの実装クラスです。
    /// </summary>
    /// <remarks>
    /// MapContextのIFieldOfViewServiceとIPathfindingServiceを
    /// 組み合わせて、視覚・空間・距離ベースの検知判定を行います。
    /// </remarks>
    public class DetectionService : IDetectionService
    {
        private readonly IFieldOfViewService _fieldOfViewService;
        private readonly IPathfindingService _pathfindingService;

        /// <summary>
        /// DetectionServiceを作成します。
        /// </summary>
        public DetectionService(
            IFieldOfViewService fieldOfViewService,
            IPathfindingService pathfindingService = null)
        {
            _fieldOfViewService = fieldOfViewService ?? throw new ArgumentNullException(nameof(fieldOfViewService));
            _pathfindingService = pathfindingService;
        }

        /// <inheritdoc/>
        public bool CanSeePlayer(Actor enemy, Actor player, RunSession session, int sightRadius)
        {
            if (enemy == null || player == null || session == null)
                return false;

            if (enemy.IsDead || player.IsDead)
                return false;

            if (sightRadius <= 0)
                return false;

            // 距離チェック（視界範囲外なら早期リターン）
            var distance = ChebyshevDistance(enemy.Position, player.Position);
            if (distance > sightRadius)
                return false;

            // 視界計算
            var visiblePositions = _fieldOfViewService.ComputeVisible(
                session.Map,
                enemy.Position,
                sightRadius);

            // プレイヤー位置が視界内にあるか
            foreach (var pos in visiblePositions)
            {
                if (pos == player.Position)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool IsInSameRoom(Actor enemy, Actor player, Map map)
        {
            if (enemy == null || player == null || map == null)
                return false;

            // 部屋情報を取得
            if (!map.TryGetRoomAt(enemy.Position, out var enemyRoom))
                return false;

            if (!map.TryGetRoomAt(player.Position, out var playerRoom))
                return false;

            // 両方が部屋にいる場合、同じ部屋かチェック
            return enemyRoom.Equals(playerRoom);
        }

        /// <inheritdoc/>
        public bool CanHearPlayer(Actor enemy, Actor player, Map map, int hearingRange)
        {
            if (enemy == null || player == null || map == null)
                return false;

            if (hearingRange <= 0)
                return false;

            // 単純な距離チェック（壁を無視）
            var distance = ChebyshevDistance(enemy.Position, player.Position);
            return distance <= hearingRange;
        }

        /// <inheritdoc/>
        public bool IsWithinWakeDistance(Actor enemy, Actor player, int wakeDistance)
        {
            if (enemy == null || player == null)
                return false;

            if (wakeDistance <= 0)
                return false;

            var distance = ChebyshevDistance(enemy.Position, player.Position);
            return distance <= wakeDistance;
        }

        /// <inheritdoc/>
        public bool CanAttackPlayer(Actor enemy, Actor player, RunSession session, int attackRange)
        {
            if (enemy == null || player == null || session == null)
                return false;

            if (enemy.IsDead || player.IsDead)
                return false;

            if (attackRange <= 0)
                return false;

            var distance = ChebyshevDistance(enemy.Position, player.Position);
            if (distance > attackRange)
                return false;

            // 近接攻撃（射程1）は視線チェック不要
            if (attackRange == 1)
                return distance == 1;

            // 遠距離攻撃は視線チェック
            if (_pathfindingService != null)
            {
                return _pathfindingService.HasLineOfSight(session.Map, enemy.Position, player.Position);
            }

            // パス探索サービスがない場合は距離だけで判定
            return true;
        }

        /// <summary>
        /// チェビシェフ距離を計算します。
        /// </summary>
        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}


