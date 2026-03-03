using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Types
{
    /// <summary>
    /// スペルアニメーション再生のための情報です。
    /// </summary>
    public readonly struct SpellAnimationRequest
    {
        public Guid CasterId { get; }
        public ItemTypeDto Spell { get; }
        public GridPositionDto CasterPosition { get; }
        public DirectionDto CasterFacing { get; }
        public GridPositionDto TargetPosition { get; }
        public Guid? TargetId { get; }
        public bool IsEquippedSpellCast { get; }

        public SpellAnimationRequest(
            Guid casterId,
            ItemTypeDto spell,
            GridPositionDto casterPosition,
            DirectionDto casterFacing,
            GridPositionDto targetPosition,
            Guid? targetId,
            bool isEquippedSpellCast)
        {
            CasterId = casterId;
            Spell = spell;
            CasterPosition = casterPosition;
            CasterFacing = casterFacing;
            TargetPosition = targetPosition;
            TargetId = targetId;
            IsEquippedSpellCast = isEquippedSpellCast;
        }
    }
}



