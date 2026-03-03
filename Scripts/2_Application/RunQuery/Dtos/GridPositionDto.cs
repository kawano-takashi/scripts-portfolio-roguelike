namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// グリッド座標を表すDTOです。
    /// </summary>
    public readonly struct GridPositionDto
    {
        public int X { get; }
        public int Y { get; }

        public GridPositionDto(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
