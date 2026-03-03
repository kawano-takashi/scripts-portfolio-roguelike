// =============================================================================
// MeleeAttackBehavior.cs
// =============================================================================
// 概要:
//   敵の近接攻撃行動を実装するクラス。プレイヤーが隣接しているときに攻撃します。
//
// 優先度: 60
//   - 遠距離攻撃（55）より高い優先度
//   - 追跡（40）より高い優先度
//   - 逃走（85）より低い優先度
//
// 実行条件:
//   - AI状態がAttacking
//   - プレイヤーとの距離が1（8方向で隣接）
//   - 攻撃射程が1（近接タイプ）
//
// 攻撃処理:
//   - AttackActionを生成してTurnResolverに返す
//   - プレイヤーの方向を向いてから攻撃
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
    /// 近接攻撃行動を実装するクラスです。
    /// </summary>
    /// <remarks>
    /// プレイヤーが隣接（距離1）しているときに攻撃します。
    /// 優先度60で、攻撃可能なら追跡より優先されます。
    /// </remarks>
    public class MeleeAttackBehavior : IEnemyBehavior
    {
        private readonly IDetectionService _detectionService;

        /// <summary>
        /// MeleeAttackBehaviorを作成します。
        /// </summary>
        public MeleeAttackBehavior(IDetectionService detectionService = null)
        {
            _detectionService = detectionService;
        }

        /// <inheritdoc/>
        public int Priority => 60;

        /// <inheritdoc/>
        public bool CanExecute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            // 攻撃状態または追跡状態で
            if (memory.CurrentState != AiState.Attacking && memory.CurrentState != AiState.Pursuing)
                return false;

            // 近接攻撃タイプで
            if (profile.AttackRange > 1)
                return false;

            var player = session.Player;
            if (player == null || player.IsDead)
                return false;

            // プレイヤーが隣接している
            var distance = ChebyshevDistance(enemy.Position, player.Position);
            return distance == 1;
        }

        /// <inheritdoc/>
        public RoguelikeAction Execute(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var player = session.Player;
            return new AttackAction(enemy.Id, player.Id, AttackKind.Melee, 1);
        }

        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}




