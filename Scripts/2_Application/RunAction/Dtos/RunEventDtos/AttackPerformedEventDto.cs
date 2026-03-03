using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct AttackPerformedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.AttackPerformed;
        public int TurnNumber { get; }
        public Guid AttackerActorId { get; }
        public Guid TargetActorId { get; }
        public int AttackKindValue { get; }
        public AttackKindDto AttackKind => (AttackKindDto)AttackKindValue;
        public int AttackSourceValue { get; }
        public AttackSourceDto AttackSource => (AttackSourceDto)AttackSourceValue;
        public GridPositionDto AttackerPosition { get; }
        public GridPositionDto TargetPosition { get; }

        public AttackPerformedEventDto(
            int turnNumber,
            Guid attackerActorId,
            Guid targetActorId,
            int attackKindValue,
            int attackSourceValue,
            GridPositionDto attackerPosition,
            GridPositionDto targetPosition)
        {
            TurnNumber = turnNumber;
            AttackerActorId = attackerActorId;
            TargetActorId = targetActorId;
            AttackKindValue = attackKindValue;
            AttackSourceValue = attackSourceValue;
            AttackerPosition = attackerPosition;
            TargetPosition = targetPosition;
        }
    }
}
