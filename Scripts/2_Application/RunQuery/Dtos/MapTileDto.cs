using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// マップタイルの読み取り専用DTOです。
    /// </summary>
    public readonly struct MapTileDto
    {
        public GridPositionDto Position { get; }
        public int TileTypeValue { get; }
        public TileTypeDto TileType => (TileTypeDto)TileTypeValue;
        public bool IsExplored { get; }
        public bool IsVisible { get; }

        public MapTileDto(
            GridPositionDto position,
            int tileTypeValue,
            bool isExplored,
            bool isVisible)
        {
            Position = position;
            TileTypeValue = tileTypeValue;
            IsExplored = isExplored;
            IsVisible = isVisible;
        }
    }
}
