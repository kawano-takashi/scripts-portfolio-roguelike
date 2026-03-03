using System;
using System.Linq;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// RunPopulationService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunPopulationServiceTests
    {
        // 観点: Populate_Throws_WhenSessionIsNull の期待挙動を検証する。
        [Fact]
        public void Populate_Throws_WhenSessionIsNull()
        {
            var sut = new RunPopulationService();

            Assert.Throws<ArgumentNullException>(() => sut.Populate(null));
        }

        // 観点: Populate_AssignsRoomRoles_AndCreatesDomainEntities の期待挙動を検証する。
        [Fact]
        public void Populate_AssignsRoomRoles_AndCreatesDomainEntities()
        {
            var roomA = new MapRect(1, 1, 4, 4);
            var roomB = new MapRect(6, 1, 4, 4);
            var roomC = new MapRect(1, 6, 4, 4);

            var map = DomainTestFactory.CreateMap(
                width: 12,
                height: 12,
                floorTiles: AllFloor(roomA).Concat(AllFloor(roomB)).Concat(AllFloor(roomC)),
                rooms: new[] { roomA, roomB, roomC },
                start: new Position(2, 2),
                stairs: new Position(7, 2));

            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player, floor: 5, seed: 77);
            var sut = new RunPopulationService();

            sut.Populate(session);

            Assert.Equal(map.Rooms.Count, session.RoomAssignments.Count);
            Assert.All(session.Enemies, enemy => Assert.Equal(Faction.Enemy, enemy.Faction));
            Assert.All(session.Items, item => Assert.NotEqual(Guid.Empty, item.Id.Value));
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
