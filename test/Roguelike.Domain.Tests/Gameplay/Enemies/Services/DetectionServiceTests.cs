using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// DetectionService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class DetectionServiceTests
    {
        // 観点: CanSeePlayer_ReturnsTrue_WhenPlayerIsInVisibleRange の期待挙動を検証する。
        [Fact]
        public void CanSeePlayer_ReturnsTrue_WhenPlayerIsInVisibleRange()
        {
            var map = CreateFloorMap(8, 8);
            var session = DomainTestFactory.CreateRunSession(
                map: map,
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(3, 3)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(1, 1));
            var sut = new DetectionService(new FieldOfViewService());

            var result = sut.CanSeePlayer(enemy, session.Player, session, sightRadius: 6);

            Assert.True(result);
        }

        // 観点: CanSeePlayer_ReturnsFalse_WhenEnemyOrPlayerIsDead の期待挙動を検証する。
        [Fact]
        public void CanSeePlayer_ReturnsFalse_WhenEnemyOrPlayerIsDead()
        {
            var map = CreateFloorMap(6, 6);
            var session = DomainTestFactory.CreateRunSession(
                map: map,
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 3));
            enemy.ApplyDamage(999);
            var sut = new DetectionService(new FieldOfViewService());

            var result = sut.CanSeePlayer(enemy, session.Player, session, sightRadius: 5);

            Assert.False(result);
        }

        // 観点: IsInSameRoom_ReturnsTrue_WhenActorsShareRoom の期待挙動を検証する。
        [Fact]
        public void IsInSameRoom_ReturnsTrue_WhenActorsShareRoom()
        {
            var room = new MapRect(1, 1, 4, 4);
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: AllFloor(room),
                rooms: new[] { room });
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 2));
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(3, 3));
            var sut = new DetectionService(new FieldOfViewService());

            var result = sut.IsInSameRoom(enemy, player, map);

            Assert.True(result);
        }

        // 観点: CanHearPlayer_UsesChebyshevDistance の期待挙動を検証する。
        [Fact]
        public void CanHearPlayer_UsesChebyshevDistance()
        {
            var map = CreateFloorMap(8, 8);
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(1, 1));
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(3, 2));
            var sut = new DetectionService(new FieldOfViewService());

            Assert.True(sut.CanHearPlayer(enemy, player, map, hearingRange: 2));
            Assert.False(sut.CanHearPlayer(enemy, player, map, hearingRange: 1));
        }

        // 観点: CanAttackPlayer_ForRangedAttack_UsesLineOfSightCheckWhenServiceProvided の期待挙動を検証する。
        [Fact]
        public void CanAttackPlayer_ForRangedAttack_UsesLineOfSightCheckWhenServiceProvided()
        {
            var map = CreateFloorMap(8, 8);
            var session = DomainTestFactory.CreateRunSession(
                map: map,
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(5, 1)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(1, 1));
            var pathfinding = new StubPathfindingService(canSee: false);
            var sut = new DetectionService(new FieldOfViewService(), pathfinding);

            var result = sut.CanAttackPlayer(enemy, session.Player, session, attackRange: 6);

            Assert.False(result);
        }

        private static Map CreateFloorMap(int width, int height)
        {
            var map = new Map(width, height);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    map.SetTileType(new Position(x, y), TileType.Floor);
                }
            }

            return map;
        }

        private static IEnumerable<Position> AllFloor(MapRect room)
        {
            for (var x = room.Left; x <= room.Right; x++)
            {
                for (var y = room.Top; y <= room.Bottom; y++)
                {
                    yield return new Position(x, y);
                }
            }
        }

        private sealed class StubPathfindingService : IPathfindingService
        {
            private readonly bool _canSee;

            public StubPathfindingService(bool canSee)
            {
                _canSee = canSee;
            }

            public IReadOnlyList<Position> FindPath(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false, int maxSearchDistance = 50)
            {
                return new List<Position>();
            }

            public Position? GetNextStep(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false)
            {
                return null;
            }

            public Position? GetFleeStep(Map map, Position start, Position threat, ISet<Position> occupiedPositions = null)
            {
                return null;
            }

            public bool HasLineOfSight(Map map, Position from, Position to)
            {
                return _canSee;
            }

            public int ChebyshevDistance(Position a, Position b)
            {
                return System.Math.Max(System.Math.Abs(a.X - b.X), System.Math.Abs(a.Y - b.Y));
            }

            public int ManhattanDistance(Position a, Position b)
            {
                return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
            }
        }
    }
}
