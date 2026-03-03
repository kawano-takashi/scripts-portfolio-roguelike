using Roguelike.Application.Enums;
using Roguelike.Domain.Gameplay.Actors.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// Application DirectionDto を Domain Direction へ変換します。
    /// </summary>
    internal static class DirectionMapper
    {
        public static Direction ToDomain(DirectionDto direction)
        {
            return direction switch
            {
                DirectionDto.Up => Direction.Up,
                DirectionDto.UpRight => Direction.UpRight,
                DirectionDto.Right => Direction.Right,
                DirectionDto.DownRight => Direction.DownRight,
                DirectionDto.Down => Direction.Down,
                DirectionDto.DownLeft => Direction.DownLeft,
                DirectionDto.Left => Direction.Left,
                DirectionDto.UpLeft => Direction.UpLeft,
                _ => Direction.Down
            };
        }
    }
}
