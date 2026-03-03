using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 現在ランの読み取り専用スナップショットです。
    /// </summary>
    public readonly struct RunSnapshotDto
    {
        public static RunSnapshotDto Empty => new RunSnapshotDto(
            hasRun: false,
            isActiveRun: false,
            floor: 0,
            clearFloor: 0,
            turnCount: 0,
            phase: RunPhaseDto.None,
            playerId: Guid.Empty,
            playerLevel: 1,
            playerCurrentHp: 0,
            playerMaxHp: 0,
            playerCurrentHunger: 0f,
            playerMaxHunger: 0f,
            playerCurrentExp: 0,
            playerExpToNextLevel: 0,
            playerPosition: default,
            playerFacingValue: 0);

        public bool HasRun { get; }
        public bool IsActiveRun { get; }
        public int Floor { get; }
        public int ClearFloor { get; }
        public int TurnCount { get; }
        public RunPhaseDto Phase { get; }
        public Guid PlayerId { get; }
        public int PlayerLevel { get; }
        public int PlayerCurrentHp { get; }
        public int PlayerMaxHp { get; }
        public float PlayerCurrentHunger { get; }
        public float PlayerMaxHunger { get; }
        public int PlayerCurrentExp { get; }
        public int PlayerExpToNextLevel { get; }
        public GridPositionDto PlayerPosition { get; }
        public int PlayerFacingValue { get; }
        public DirectionDto PlayerFacing => (DirectionDto)PlayerFacingValue;

        public RunSnapshotDto(
            bool hasRun,
            bool isActiveRun,
            int floor,
            int clearFloor,
            int turnCount,
            RunPhaseDto phase,
            Guid playerId,
            int playerLevel,
            int playerCurrentHp,
            int playerMaxHp,
            float playerCurrentHunger,
            float playerMaxHunger,
            int playerCurrentExp,
            int playerExpToNextLevel,
            GridPositionDto playerPosition,
            int playerFacingValue)
        {
            HasRun = hasRun;
            IsActiveRun = isActiveRun;
            Floor = floor;
            ClearFloor = clearFloor;
            TurnCount = turnCount;
            Phase = phase;
            PlayerId = playerId;
            PlayerLevel = playerLevel;
            PlayerCurrentHp = playerCurrentHp;
            PlayerMaxHp = playerMaxHp;
            PlayerCurrentHunger = playerCurrentHunger;
            PlayerMaxHunger = playerMaxHunger;
            PlayerCurrentExp = playerCurrentExp;
            PlayerExpToNextLevel = playerExpToNextLevel;
            PlayerPosition = playerPosition;
            PlayerFacingValue = playerFacingValue;
        }
    }
}
