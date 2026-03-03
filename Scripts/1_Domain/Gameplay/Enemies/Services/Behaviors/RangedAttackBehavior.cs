// =============================================================================
// RangedAttackBehavior.cs
// =============================================================================
// 概要:
//   敵の遠距離攻撃行動を実装するクラス。射程内かつ視線が通るときに攻撃します。
//
// 優先度: 55
//   - 近接攻撃（60）より低い優先度（近接できるなら近接優先）
//   - 追跡（40）より高い優先度
//   - 逃走（85）より低い優先度
//
// 実行条件:
//   - AI状態がAttacking
//   - プレイヤーとの距離がAttackRange以内
//   - 攻撃射程が2以上（遠距離タイプ）
//   - 視線が通っている（HasLineOfSight）
//   - SpecialAbility.RangedAttackを持っている
//
// 射線チェック:
//   - IPathfindingService.HasLineOfSight()を使用
//   - 壁越しの攻撃を防止
// =============================================================================

using System;
using Roguelike.Domain.Gameplay.Enemies.Entities;
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
    /// 遠距離攻撃行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// プレイヤーが射程内にいて視線が通っているときに攻撃します。
    /// 優先度55で、近接攻撃ができない場合に選択されます。
    /// </remarks>
    public class RangedAttackBehavior : IEnemyBehavior
    {
        private readonly IPathfindingService _pathfindingService;
        private readonly IDetectionService _detectionService;

        /// <summary>
        /// RangedAttackBehaviorを作成します。
        /// </summary>
        public RangedAttackBehavior(
            IPathfindingService pathfindingService = null,
            IDetectionService detectionService = null)
        {
            _pathfindingService = pathfindingService;
            _detectionService = detectionService;
        }

        /// <inheritdoc/>
        public int Priority => 55;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 攻撃状態または追跡状態で
            if (memory.CurrentState != AiState.Attacking && memory.CurrentState != AiState.Pursuing)
                return false;

            // 遠距離攻撃タイプで
            if (profile.AttackRange <= 1)
                return false;

            var player = session.Player;
            if (player == null || player.IsDead)
                return false;

            var distance = ChebyshevDistance(enemy.Position, player.Position);

            // 射程内で
            if (distance > profile.AttackRange)
                return false;

            // 近すぎない（好む間合いより近い場合は攻撃しない）
            if (distance < profile.PreferredDistance)
                return false;

            // 視線が通っている
            if (_pathfindingService != null)
            {
                return _pathfindingService.HasLineOfSight(session.Map, enemy.Position, player.Position);
            }

            return true;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var player = session.Player;
            return new AttackAction(enemy.Id, player.Id, AttackKind.Ranged, profile.AttackRange);
        }

        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}




