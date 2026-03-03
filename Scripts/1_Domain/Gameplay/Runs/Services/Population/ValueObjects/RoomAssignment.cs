using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// 部屋と役割の割り当てを表す値オブジェクトです。
    /// </summary>
    public readonly struct RoomAssignment
    {
        /// <summary>
        /// 部屋の範囲。
        /// </summary>
        public MapRect Room { get; }

        /// <summary>
        /// 部屋の役割。
        /// </summary>
        public RoomRole Role { get; }

        /// <summary>
        /// RoomAssignmentを作成します。
        /// </summary>
        public RoomAssignment(MapRect room, RoomRole role)
        {
            Room = room;
            Role = role;
        }
    }
}


