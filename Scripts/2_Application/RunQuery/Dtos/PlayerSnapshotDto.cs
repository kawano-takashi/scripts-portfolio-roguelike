using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// プレイヤー表示用スナップショットDTOです。
    /// </summary>
    public readonly struct PlayerSnapshotDto
    {
        public Guid ActorId { get; }
        public string Name { get; }
        public GridPositionDto Position { get; }
        public int FacingValue { get; }
        public DirectionDto Facing => (DirectionDto)FacingValue;
        public int Level { get; }

        public PlayerSnapshotDto(Guid actorId, string name, GridPositionDto position, int facingValue, int level)
        {
            ActorId = actorId;
            Name = name;
            Position = position;
            FacingValue = facingValue;
            Level = level;
        }
    }
}
