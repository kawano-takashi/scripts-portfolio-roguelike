using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;

namespace Roguelike.Presentation.Gameplay.Map.DisplayModels
{
    /// <summary>
    /// ミニマップ描画向けの表示モデルです。
    /// </summary>
    public sealed class MiniMapDisplayModel
    {
        private static readonly IReadOnlyList<EnemySnapshotDto> EmptyEnemies = Array.Empty<EnemySnapshotDto>();
        private static readonly IReadOnlyList<GroundItemSnapshotDto> EmptyGroundItems =
            Array.Empty<GroundItemSnapshotDto>();

        public MapSnapshotDto Map { get; }
        public PlayerSnapshotDto Player { get; }
        public IReadOnlyList<EnemySnapshotDto> Enemies { get; }
        public IReadOnlyList<GroundItemSnapshotDto> GroundItems { get; }

        public MiniMapDisplayModel(
            MapSnapshotDto map,
            PlayerSnapshotDto player,
            IReadOnlyList<EnemySnapshotDto> enemies,
            IReadOnlyList<GroundItemSnapshotDto> groundItems)
        {
            Map = map;
            Player = player;
            Enemies = enemies ?? EmptyEnemies;
            GroundItems = groundItems ?? EmptyGroundItems;
        }
    }
}



