using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Maps.Entities
{
    /// <summary>
    /// Map の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class MapTests
    {
        // 観点: Constructor_Throws_WhenSizeIsInvalid の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenSizeIsInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Map(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Map(1, 0));
        }

        // 観点: SetStartPosition_Throws_WhenOutsideMap の期待挙動を検証する。
        [Fact]
        public void SetStartPosition_Throws_WhenOutsideMap()
        {
            var sut = new Map(4, 4);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetStartPosition(new Position(9, 9)));
        }

        // 観点: SetStairsDownPosition_Throws_WhenOutsideMap の期待挙動を検証する。
        [Fact]
        public void SetStairsDownPosition_Throws_WhenOutsideMap()
        {
            var sut = new Map(4, 4);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetStairsDownPosition(new Position(-1, 0)));
        }

        // 観点: GetTile_ReturnsWall_WhenOutsideMap の期待挙動を検証する。
        [Fact]
        public void GetTile_ReturnsWall_WhenOutsideMap()
        {
            var sut = new Map(4, 4);

            var tile = sut.GetTile(new Position(99, 99));

            Assert.Equal(TileType.Wall, tile.Type);
            Assert.True(tile.BlocksSight);
        }

        // 観点: SetTileType_SetsFirstFloorAsStartPosition_WhenStartIsUnset の期待挙動を検証する。
        [Fact]
        public void SetTileType_SetsFirstFloorAsStartPosition_WhenStartIsUnset()
        {
            var sut = new Map(4, 4);

            sut.SetTileType(new Position(2, 1), TileType.Floor);

            Assert.Equal(new Position(2, 1), sut.StartPosition);
        }

        // 観点: ApplyVisibility_UpdatesVisibleAndExploredFlags の期待挙動を検証する。
        [Fact]
        public void ApplyVisibility_UpdatesVisibleAndExploredFlags()
        {
            var sut = CreateFloorMap(4, 4);
            sut.SetTile(new Position(0, 0), new Tile(TileType.Floor, isExplored: true, isVisible: true));

            sut.ApplyVisibility(new[] { new Position(1, 1) });

            Assert.False(sut.GetTile(new Position(0, 0)).IsVisible);
            Assert.True(sut.GetTile(new Position(0, 0)).IsExplored);
            Assert.True(sut.GetTile(new Position(1, 1)).IsVisible);
            Assert.True(sut.GetTile(new Position(1, 1)).IsExplored);
        }

        // 観点: ApplyVisibilityByRoomOrRadius_RevealsRoom_WhenOriginIsInRoom の期待挙動を検証する。
        [Fact]
        public void ApplyVisibilityByRoomOrRadius_RevealsRoom_WhenOriginIsInRoom()
        {
            var sut = CreateFloorMap(6, 6);
            sut.SetRooms(new[] { new MapRect(1, 1, 3, 3) });

            sut.ApplyVisibilityByRoomOrRadius(new Position(2, 2), corridorRadius: 1);

            Assert.True(sut.GetTile(new Position(1, 1)).IsVisible);
            Assert.True(sut.GetTile(new Position(3, 3)).IsVisible);
            Assert.False(sut.GetTile(new Position(5, 5)).IsVisible);
        }

        // 観点: ApplyVisibilityByRoomOrRadius_RevealsSquareRadius_WhenOriginIsCorridor の期待挙動を検証する。
        [Fact]
        public void ApplyVisibilityByRoomOrRadius_RevealsSquareRadius_WhenOriginIsCorridor()
        {
            var sut = CreateFloorMap(7, 7);

            sut.ApplyVisibilityByRoomOrRadius(new Position(3, 3), corridorRadius: 1);

            Assert.True(sut.GetTile(new Position(2, 2)).IsVisible);
            Assert.True(sut.GetTile(new Position(4, 4)).IsVisible);
            Assert.False(sut.GetTile(new Position(0, 0)).IsVisible);
        }

        // 観点: RevealAll_MarksAllTilesVisibleAndExplored の期待挙動を検証する。
        [Fact]
        public void RevealAll_MarksAllTilesVisibleAndExplored()
        {
            var sut = CreateFloorMap(3, 2);

            sut.RevealAll();

            foreach (var position in sut.AllPositions())
            {
                var tile = sut.GetTile(position);
                Assert.True(tile.IsVisible);
                Assert.True(tile.IsExplored);
            }
        }

        // 観点: Rooms_ReturnsReadOnlyCollection の期待挙動を検証する。
        [Fact]
        public void Rooms_ReturnsReadOnlyCollection()
        {
            var sut = CreateFloorMap(6, 6);
            sut.SetRooms(new[] { new MapRect(1, 1, 3, 3) });
            var rooms = Assert.IsAssignableFrom<IList<MapRect>>(sut.Rooms);

            Assert.Throws<NotSupportedException>(() => rooms.Add(new MapRect(0, 0, 1, 1)));
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
