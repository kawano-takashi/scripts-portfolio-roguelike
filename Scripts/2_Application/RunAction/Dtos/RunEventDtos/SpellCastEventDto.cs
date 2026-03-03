using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct SpellCastEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.SpellCast;
        public int TurnNumber { get; }
        public Guid CasterActorId { get; }
        public Guid? TargetActorId { get; }
        public int ItemTypeValue { get; }
        public ItemTypeDto ItemType => (ItemTypeDto)ItemTypeValue;
        public int DirectionValue { get; }
        public DirectionDto Direction => (DirectionDto)DirectionValue;
        public int Range { get; }
        public bool IsEquippedSpellCast { get; }
        public GridPositionDto CasterPosition { get; }
        public GridPositionDto TargetPosition { get; }

        public SpellCastEventDto(
            int turnNumber,
            Guid casterActorId,
            Guid? targetActorId,
            int itemTypeValue,
            int directionValue,
            int range,
            bool isEquippedSpellCast,
            GridPositionDto casterPosition,
            GridPositionDto targetPosition)
        {
            TurnNumber = turnNumber;
            CasterActorId = casterActorId;
            TargetActorId = targetActorId;
            ItemTypeValue = itemTypeValue;
            DirectionValue = directionValue;
            Range = range;
            IsEquippedSpellCast = isEquippedSpellCast;
            CasterPosition = casterPosition;
            TargetPosition = targetPosition;
        }
    }
}
