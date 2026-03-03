using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// SimplePathfindingService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SimplePathfindingServiceTests
    {
        // 観点: FindPath_ReturnsEmpty_WhenMapIsNull の期待挙動を検証する。
        [Fact]
        public void FindPath_ReturnsEmpty_WhenMapIsNull()
        {
            var sut = new SimplePathfindingService();

            var path = sut.FindPath(null, Position.Zero, new Position(1, 1));

            Assert.Empty(path);
        }

        // 観点: FindPath_ReturnsPath_OnOpenMap の期待挙動を検証する。
        [Fact]
        public void FindPath_ReturnsPath_OnOpenMap()
        {
            var sut = new SimplePathfindingService();
            var map = CreateFloorMap(8, 8);
            var start = new Position(1, 1);
            var goal = new Position(4, 1);

            var path = sut.FindPath(map, start, goal);

            Assert.NotEmpty(path);
            Assert.Equal(goal, path[^1]);
            Assert.Equal(3, path.Count);
            Assert.Equal(1, sut.ChebyshevDistance(start, path[0]));
        }

        // 観点: GetNextStep_ReturnsNull_WhenNoPathExists の期待挙動を検証する。
        [Fact]
        public void GetNextStep_ReturnsNull_WhenNoPathExists()
        {
            var sut = new SimplePathfindingService();
            var map = new Map(4, 4);
            map.SetTileType(new Position(1, 1), TileType.Floor);
            map.SetTileType(new Position(2, 2), TileType.Floor);

            var step = sut.GetNextStep(map, new Position(1, 1), new Position(2, 2));

            Assert.Null(step);
        }

        // 観点: FindPath_RespectsOccupiedCells_ButCanAllowOccupiedGoal の期待挙動を検証する。
        [Fact]
        public void FindPath_RespectsOccupiedCells_ButCanAllowOccupiedGoal()
        {
            var sut = new SimplePathfindingService();
            var map = CreateFloorMap(6, 3);
            var occupied = new HashSet<Position> { new Position(4, 1) };

            var blockedPath = sut.FindPath(map, new Position(1, 1), new Position(4, 1), occupiedPositions: occupied, allowOccupiedGoal: false);
            var allowedPath = sut.FindPath(map, new Position(1, 1), new Position(4, 1), occupiedPositions: occupied, allowOccupiedGoal: true);

            Assert.Empty(blockedPath);
            Assert.NotEmpty(allowedPath);
            Assert.Equal(new Position(4, 1), allowedPath[^1]);
        }

        // 観点: GetFleeStep_SelectsPositionFartherFromThreat の期待挙動を検証する。
        [Fact]
        public void GetFleeStep_SelectsPositionFartherFromThreat()
        {
            var sut = new SimplePathfindingService();
            var map = CreateFloorMap(8, 8);

            var next = sut.GetFleeStep(map, start: new Position(4, 4), threat: new Position(4, 5));

            Assert.True(next.HasValue);
            Assert.True(sut.ChebyshevDistance(next.Value, new Position(4, 5)) > 1);
        }

        // 観点: HasLineOfSight_ReturnsFalse_WhenWallBlocksLine の期待挙動を検証する。
        [Fact]
        public void HasLineOfSight_ReturnsFalse_WhenWallBlocksLine()
        {
            var sut = new SimplePathfindingService();
            var map = CreateFloorMap(8, 3);
            map.SetTileType(new Position(3, 1), TileType.Wall);

            var canSee = sut.HasLineOfSight(map, new Position(1, 1), new Position(6, 1));

            Assert.False(canSee);
        }

        // 観点: DistanceMethods_ReturnExpectedValues の期待挙動を検証する。
        [Fact]
        public void DistanceMethods_ReturnExpectedValues()
        {
            var sut = new SimplePathfindingService();

            Assert.Equal(3, sut.ChebyshevDistance(new Position(1, 1), new Position(4, 3)));
            Assert.Equal(5, sut.ManhattanDistance(new Position(1, 1), new Position(4, 3)));
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
    }
}
