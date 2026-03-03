using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// 部屋に役割を割り当てるサービスです。
    /// </summary>
    public sealed class RoomRoleAssigner
    {
        /// <summary>
        /// マップの各部屋に役割を割り当てます。
        /// </summary>
        public IReadOnlyList<RoomAssignment> Assign(Map map, FloorProfile profile, Position playerPosition)
        {
            if (map == null || map.Rooms.Count == 0)
            {
                return Array.Empty<RoomAssignment>();
            }

            var assignments = new List<RoomAssignment>();
            var rooms = map.Rooms;
            var stairsPosition = map.StairsDownPosition;

            MapRect? startRoom = null;
            MapRect? stairsRoom = null;
            MapRect? monsterHouseRoom = null;

            // スタート部屋と階段部屋を特定
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];

                if (room.Contains(playerPosition))
                {
                    startRoom = room;
                }

                if (stairsPosition.HasValue && room.Contains(stairsPosition.Value))
                {
                    stairsRoom = room;
                }
            }

            // モンスターハウスの部屋を選択（最大面積の部屋、ただしスタート・階段部屋は除く）
            if (profile.HasMonsterHouse)
            {
                int maxArea = 0;
                for (int i = 0; i < rooms.Count; i++)
                {
                    var room = rooms[i];

                    // スタート部屋と階段部屋は除外
                    if (startRoom.HasValue && room.Equals(startRoom.Value))
                    {
                        continue;
                    }

                    if (stairsRoom.HasValue && room.Equals(stairsRoom.Value))
                    {
                        continue;
                    }

                    var area = room.Width * room.Height;
                    if (area > maxArea)
                    {
                        maxArea = area;
                        monsterHouseRoom = room;
                    }
                }
            }

            // 各部屋に役割を割り当て
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                RoomRole role;

                if (startRoom.HasValue && room.Equals(startRoom.Value))
                {
                    role = RoomRole.Start;
                }
                else if (stairsRoom.HasValue && room.Equals(stairsRoom.Value))
                {
                    role = RoomRole.Stairs;
                }
                else if (monsterHouseRoom.HasValue && room.Equals(monsterHouseRoom.Value))
                {
                    role = RoomRole.MonsterHouse;
                }
                else
                {
                    role = RoomRole.Normal;
                }

                assignments.Add(new RoomAssignment(room, role));
            }

            return assignments;
        }
    }
}


