using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Maps.Services
{
    /// <summary>
    /// FieldOfViewService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class FieldOfViewServiceTests
    {
        // 観点: ComputeVisible_ReturnsEmpty_WhenMapIsNull の期待挙動を検証する。
        [Fact]
        public void ComputeVisible_ReturnsEmpty_WhenMapIsNull()
        {
            var sut = new FieldOfViewService();

            var visible = sut.ComputeVisible(map: null, origin: Position.Zero, radius: 3);

            Assert.Empty(visible);
        }

        // 観点: ComputeVisible_ReturnsEmpty_WhenOriginIsOutsideMap の期待挙動を検証する。
        [Fact]
        public void ComputeVisible_ReturnsEmpty_WhenOriginIsOutsideMap()
        {
            var sut = new FieldOfViewService();
            var map = CreateFloorMap(5, 5);

            var visible = sut.ComputeVisible(map, new Position(10, 10), radius: 3);

            Assert.Empty(visible);
        }

        // 観点: ComputeVisible_ReturnsOnlyOrigin_WhenRadiusIsZero の期待挙動を検証する。
        [Fact]
        public void ComputeVisible_ReturnsOnlyOrigin_WhenRadiusIsZero()
        {
            var sut = new FieldOfViewService();
            var map = CreateFloorMap(5, 5);

            var visible = sut.ComputeVisible(map, new Position(2, 2), radius: 0);

            Assert.Single(visible);
            Assert.Contains(new Position(2, 2), visible);
        }

        // 観点: ComputeVisible_ContainsTilesWithinRadius_OnOpenMap の期待挙動を検証する。
        [Fact]
        public void ComputeVisible_ContainsTilesWithinRadius_OnOpenMap()
        {
            var sut = new FieldOfViewService();
            var map = CreateFloorMap(7, 7);

            var visible = sut.ComputeVisible(map, new Position(3, 3), radius: 2);

            Assert.Contains(new Position(3, 3), visible);
            Assert.Contains(new Position(4, 3), visible);
            Assert.Contains(new Position(2, 4), visible);
        }

        // 観点: ComputeVisible_DoesNotSeeBeyondWall_OnSameLine の期待挙動を検証する。
        [Fact]
        public void ComputeVisible_DoesNotSeeBeyondWall_OnSameLine()
        {
            var sut = new FieldOfViewService();
            var map = CreateFloorMap(7, 3);
            map.SetTileType(new Position(3, 1), TileType.Wall);

            var visible = new HashSet<Position>(sut.ComputeVisible(map, new Position(1, 1), radius: 6));

            Assert.Contains(new Position(3, 1), visible);
            Assert.DoesNotContain(new Position(5, 1), visible);
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
