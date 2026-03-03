using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 敵表示用スナップショットDTOです。
    /// </summary>
    public readonly struct EnemySnapshotDto
    {
        public Guid ActorId { get; }
        public string Name { get; }
        public GridPositionDto Position { get; }
        public int FacingValue { get; }
        public DirectionDto Facing => (DirectionDto)FacingValue;
        public int? EnemyArchetypeValue { get; }
        public EnemyArchetypeDto EnemyArchetype =>
            EnemyArchetypeValue.HasValue ? (EnemyArchetypeDto)EnemyArchetypeValue.Value : EnemyArchetypeDto.Melee;

        public EnemySnapshotDto(
            Guid actorId,
            string name,
            GridPositionDto position,
            int facingValue,
            int? enemyArchetypeValue)
        {
            ActorId = actorId;
            Name = name;
            Position = position;
            FacingValue = facingValue;
            EnemyArchetypeValue = enemyArchetypeValue;
        }
    }
}
