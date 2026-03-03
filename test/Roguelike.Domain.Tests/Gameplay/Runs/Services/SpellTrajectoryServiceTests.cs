using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// SpellTrajectoryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SpellTrajectoryServiceTests
    {
        // 観点: BuildLinearTrajectory_ReturnsEmpty_WhenMapIsNull の期待挙動を検証する。
        [Fact]
        public void BuildLinearTrajectory_ReturnsEmpty_WhenMapIsNull()
        {
            var sut = new SpellTrajectoryService();

            var trajectory = sut.BuildLinearTrajectory(null, Position.Zero, Direction.Right, range: 3);

            Assert.Empty(trajectory);
        }

        // 観点: BuildLinearTrajectory_ReturnsEmpty_WhenRangeIsNotPositive の期待挙動を検証する。
        [Fact]
        public void BuildLinearTrajectory_ReturnsEmpty_WhenRangeIsNotPositive()
        {
            var sut = new SpellTrajectoryService();
            var map = CreateFloorMap(5, 5);

            var trajectory = sut.BuildLinearTrajectory(map, new Position(1, 1), Direction.Right, range: 0);

            Assert.Empty(trajectory);
        }

        // 観点: BuildLinearTrajectory_StopsAtMapBoundary の期待挙動を検証する。
        [Fact]
        public void BuildLinearTrajectory_StopsAtMapBoundary()
        {
            var sut = new SpellTrajectoryService();
            var map = CreateFloorMap(4, 4);

            var trajectory = sut.BuildLinearTrajectory(map, new Position(2, 1), Direction.Right, range: 5);

            var first = Assert.Single(trajectory);
            Assert.Equal(new Position(3, 1), first);
        }

        // 観点: BuildLinearTrajectory_StopsBeforeBlockingTile の期待挙動を検証する。
        [Fact]
        public void BuildLinearTrajectory_StopsBeforeBlockingTile()
        {
            var sut = new SpellTrajectoryService();
            var map = CreateFloorMap(6, 3);
            map.SetTileType(new Position(3, 1), TileType.Wall);

            var trajectory = sut.BuildLinearTrajectory(map, new Position(1, 1), Direction.Right, range: 5);

            Assert.Single(trajectory);
            Assert.Equal(new Position(2, 1), trajectory[0]);
        }

        // 観点: BuildLinearTrajectory_SupportsDiagonalDirection の期待挙動を検証する。
        [Fact]
        public void BuildLinearTrajectory_SupportsDiagonalDirection()
        {
            var sut = new SpellTrajectoryService();
            var map = CreateFloorMap(6, 6);

            var trajectory = sut.BuildLinearTrajectory(map, new Position(1, 1), Direction.DownRight, range: 3);

            Assert.Equal(3, trajectory.Count);
            Assert.Equal(new Position(2, 2), trajectory[0]);
            Assert.Equal(new Position(4, 4), trajectory[2]);
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
