using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// ラン表示向け統合ReadModel DTOです。
    /// </summary>
    public readonly struct RunReadModelDto
    {
        private static readonly IReadOnlyList<EnemySnapshotDto> EmptyEnemies = Array.Empty<EnemySnapshotDto>();
        private static readonly IReadOnlyList<GroundItemSnapshotDto> EmptyItems = Array.Empty<GroundItemSnapshotDto>();

        public static RunReadModelDto Empty => new RunReadModelDto(
            hasRun: false,
            snapshot: RunSnapshotDto.Empty,
            map: MapSnapshotDto.Empty,
            player: default,
            enemies: EmptyEnemies,
            groundItems: EmptyItems);

        public bool HasRun { get; }
        public RunSnapshotDto Snapshot { get; }
        public MapSnapshotDto Map { get; }
        public PlayerSnapshotDto Player { get; }
        public IReadOnlyList<EnemySnapshotDto> Enemies { get; }
        public IReadOnlyList<GroundItemSnapshotDto> GroundItems { get; }

        public RunReadModelDto(
            bool hasRun,
            RunSnapshotDto snapshot,
            MapSnapshotDto map,
            PlayerSnapshotDto player,
            IReadOnlyList<EnemySnapshotDto> enemies,
            IReadOnlyList<GroundItemSnapshotDto> groundItems)
        {
            HasRun = hasRun;
            Snapshot = snapshot;
            Map = map;
            Player = player;
            Enemies = enemies ?? EmptyEnemies;
            GroundItems = groundItems ?? EmptyItems;
        }
    }
}
