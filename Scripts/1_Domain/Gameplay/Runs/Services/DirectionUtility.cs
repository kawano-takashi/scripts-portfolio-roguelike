using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// 方向→座標変換と、斜め移動の角抜け判定をまとめたヘルパーです。
    /// </summary>
    internal static class DirectionUtility
    {
        public static Position Apply(Position position, Direction direction)
        {
            // マップのY軸は「下が正」なので、Up は -1 になります。
            var offset = GetOffset(direction);
            return new Position(position.X + offset.X, position.Y + offset.Y);
        }

        public static bool CanMoveDiagonal(Map map, Position from, Direction direction)
        {
            if (!IsDiagonal(direction))
            {
                return true;
            }

            // 角抜け禁止: 斜め移動は隣接する縦横2マスが歩行可能な時だけ許可します。
            var offset = GetOffset(direction);
            var horizontal = new Position(from.X + offset.X, from.Y);
            var vertical = new Position(from.X, from.Y + offset.Y);

            return map.Contains(horizontal)
                && map.Contains(vertical)
                && map.IsWalkable(horizontal)
                && map.IsWalkable(vertical);
        }

        public static bool IsDiagonal(Direction direction)
        {
            var offset = GetOffset(direction);
            return offset.X != 0 && offset.Y != 0;
        }

        private static Position GetOffset(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Position(0, -1),
                Direction.UpRight => new Position(1, -1),
                Direction.Right => new Position(1, 0),
                Direction.DownRight => new Position(1, 1),
                Direction.Down => new Position(0, 1),
                Direction.DownLeft => new Position(-1, 1),
                Direction.Left => new Position(-1, 0),
                Direction.UpLeft => new Position(-1, -1),
                _ => Position.Zero
            };
        }
    }
}


