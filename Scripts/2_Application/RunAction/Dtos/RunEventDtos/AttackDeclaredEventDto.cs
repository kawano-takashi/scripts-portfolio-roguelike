using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct AttackDeclaredEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.AttackDeclared;
        public int TurnNumber { get; }
        public Guid AttackerActorId { get; }
        public Guid? TargetActorId { get; }
        public int AttackKindValue { get; }
        public AttackKindDto AttackKind => (AttackKindDto)AttackKindValue;
        public int DirectionValue { get; }
        public DirectionDto Direction => (DirectionDto)DirectionValue;
        public int Range { get; }
        public GridPositionDto AttackerPosition { get; }
        public GridPositionDto TargetPosition { get; }

        public AttackDeclaredEventDto(
            int turnNumber,
            Guid attackerActorId,
            Guid? targetActorId,
            int attackKindValue,
            int directionValue,
            int range,
            GridPositionDto attackerPosition,
            GridPositionDto targetPosition)
        {
            TurnNumber = turnNumber;
            AttackerActorId = attackerActorId;
            TargetActorId = targetActorId;
            AttackKindValue = attackKindValue;
            DirectionValue = directionValue;
            Range = range;
            AttackerPosition = attackerPosition;
            TargetPosition = targetPosition;
        }
    }
}
