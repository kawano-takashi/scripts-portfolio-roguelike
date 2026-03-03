using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;
namespace Roguelike.Domain.Gameplay.Maps.Enums
{
    /// <summary>
    /// マップのマスの種類です。
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// 壁（通れない）。
        /// </summary>
        Wall,
        /// <summary>
        /// 床（通れる）。
        /// </summary>
        Floor,
        /// <summary>
        /// 閉じたドア（壁と同じで通れない）。
        /// </summary>
        DoorClosed,
        /// <summary>
        /// 開いたドア（床と同じで通れる）。
        /// </summary>
        DoorOpen,
        /// <summary>
        /// 下り階段。
        /// </summary>
        StairsDown,
        /// <summary>
        /// まだ見ていない場所（未探索）。
        /// </summary>
        Unknown
    }
}


