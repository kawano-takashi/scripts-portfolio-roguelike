using System.Linq;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// RoomRoleAssigner の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RoomRoleAssignerTests
    {
        // 観点: Assign_ReturnsEmpty_WhenMapHasNoRooms の期待挙動を検証する。
        [Fact]
        public void Assign_ReturnsEmpty_WhenMapHasNoRooms()
        {
            var sut = new RoomRoleAssigner();
            var map = DomainTestFactory.CreateMap();

            var result = sut.Assign(map, new FloorProfile(1, FloorProfileType.Normal), playerPosition: new Position(1, 1));

            Assert.Empty(result);
        }

        // 観点: Assign_MarksStartAndStairsRooms の期待挙動を検証する。
        [Fact]
        public void Assign_MarksStartAndStairsRooms()
        {
            var sut = new RoomRoleAssigner();
            var roomA = new MapRect(1, 1, 3, 3);
            var roomB = new MapRect(5, 1, 3, 3);
            var map = DomainTestFactory.CreateMap(
                width: 10,
                height: 6,
                floorTiles: AllFloor(roomA).Concat(AllFloor(roomB)),
                rooms: new[] { roomA, roomB },
                start: new Position(2, 2),
                stairs: new Position(6, 2));

            var result = sut.Assign(map, new FloorProfile(1, FloorProfileType.Normal), playerPosition: new Position(2, 2));

            Assert.Equal(RoomRole.Start, result.Single(a => a.Room.Equals(roomA)).Role);
            Assert.Equal(RoomRole.Stairs, result.Single(a => a.Room.Equals(roomB)).Role);
        }

        // 観点: Assign_SelectsLargestEligibleRoomAsMonsterHouse の期待挙動を検証する。
        [Fact]
        public void Assign_SelectsLargestEligibleRoomAsMonsterHouse()
        {
            var sut = new RoomRoleAssigner();
            var startRoom = new MapRect(1, 1, 3, 3);
            var stairsRoom = new MapRect(5, 1, 3, 3);
            var monsterHouseCandidate = new MapRect(1, 5, 5, 4);
            var normalRoom = new MapRect(8, 5, 2, 2);
            var map = DomainTestFactory.CreateMap(
                width: 12,
                height: 12,
                floorTiles: AllFloor(startRoom)
                    .Concat(AllFloor(stairsRoom))
                    .Concat(AllFloor(monsterHouseCandidate))
                    .Concat(AllFloor(normalRoom)),
                rooms: new[] { startRoom, stairsRoom, monsterHouseCandidate, normalRoom },
                start: new Position(2, 2),
                stairs: new Position(6, 2));

            var result = sut.Assign(map, new FloorProfile(6, FloorProfileType.MonsterHouse), playerPosition: new Position(2, 2));

            Assert.Equal(RoomRole.MonsterHouse, result.Single(a => a.Room.Equals(monsterHouseCandidate)).Role);
            Assert.Equal(RoomRole.Normal, result.Single(a => a.Room.Equals(normalRoom)).Role);
        }

        private static System.Collections.Generic.IEnumerable<Position> AllFloor(MapRect room)
        {
            for (var x = room.Left; x <= room.Right; x++)
            {
                for (var y = room.Top; y <= room.Bottom; y++)
                {
                    yield return new Position(x, y);
                }
            }
        }
    }
}
