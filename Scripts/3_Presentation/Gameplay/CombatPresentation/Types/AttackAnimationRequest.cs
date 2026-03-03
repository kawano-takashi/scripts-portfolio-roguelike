using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Types
{
    /// <summary>
    /// 攻撃アニメーション再生のための情報です。
    /// </summary>
    public sealed class AttackAnimationRequest
    {
        public Guid AttackerId { get; }
        public Guid? TargetId { get; }
        public AttackKindDto Kind { get; }
        public GridPositionDto AttackerPosition { get; }
        public GridPositionDto TargetPosition { get; }
        public bool ShowHitEffect { get; }

        public AttackAnimationRequest(
            Guid attackerId,
            Guid? targetId,
            AttackKindDto kind,
            GridPositionDto attackerPosition,
            GridPositionDto targetPosition,
            bool showHitEffect)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Kind = kind;
            AttackerPosition = attackerPosition;
            TargetPosition = targetPosition;
            ShowHitEffect = showHitEffect;
        }
    }
}



