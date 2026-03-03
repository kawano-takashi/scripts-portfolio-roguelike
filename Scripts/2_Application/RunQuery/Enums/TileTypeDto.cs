namespace Roguelike.Application.Enums
{
    /// <summary>
    /// Application層で扱うタイル種別です。
    /// Domain.TileType と同じ並びを維持します。
    /// </summary>
    public enum TileTypeDto
    {
        Wall = 0,
        Floor = 1,
        DoorClosed = 2,
        DoorOpen = 3,
        StairsDown = 4,
        Unknown = 5
    }
}
