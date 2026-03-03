using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
namespace Roguelike.Domain.Gameplay.Actors.Enums
{
    /// <summary>
    /// 方向（どっちを向いているか）を表します。
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 上。
        /// </summary>
        Up,

        /// <summary>
        /// 右上。
        /// </summary>
        UpRight,

        /// <summary>
        /// 右。
        /// </summary>
        Right,

        /// <summary>
        /// 右下。
        /// </summary>
        DownRight,

        /// <summary>
        /// 下。
        /// </summary>
        Down,

        /// <summary>
        /// 左下。
        /// </summary>
        DownLeft,

        /// <summary>
        /// 左。
        /// </summary>
        Left,

        /// <summary>
        /// 左上。
        /// </summary>
        UpLeft
    }
}


