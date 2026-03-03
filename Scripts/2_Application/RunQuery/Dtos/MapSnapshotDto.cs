using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// マップ読み取り用スナップショットDTOです。
    /// </summary>
    public readonly struct MapSnapshotDto
    {
        private static readonly IReadOnlyList<MapTileDto> EmptyTiles = Array.Empty<MapTileDto>();

        public static MapSnapshotDto Empty => new MapSnapshotDto(
            width: 0,
            height: 0,
            startPosition: null,
            stairsDownPosition: null,
            tiles: EmptyTiles);

        public int Width { get; }
        public int Height { get; }
        public GridPositionDto? StartPosition { get; }
        public GridPositionDto? StairsDownPosition { get; }
        public IReadOnlyList<MapTileDto> Tiles { get; }

        public MapSnapshotDto(
            int width,
            int height,
            GridPositionDto? startPosition,
            GridPositionDto? stairsDownPosition,
            IReadOnlyList<MapTileDto> tiles)
        {
            Width = width;
            Height = height;
            StartPosition = startPosition;
            StairsDownPosition = stairsDownPosition;
            Tiles = tiles ?? EmptyTiles;
        }
    }
}
