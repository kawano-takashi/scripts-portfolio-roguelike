using Roguelike.Domain.Gameplay.Maps.Services;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Maps.Services
{
    /// <summary>
    /// MapGenerationService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class MapGenerationServiceTests
    {
        // 観点: Generate_UsesDefaultSize_WhenOnlySeedIsProvided の期待挙動を検証する。
        [Fact]
        public void Generate_UsesDefaultSize_WhenOnlySeedIsProvided()
        {
            var sut = new MapGenerationService();

            var map = sut.Generate(seed: 1234);

            Assert.Equal(MapGenerationService.DefaultWidth, map.Size.Width);
            Assert.Equal(MapGenerationService.DefaultHeight, map.Size.Height);
            Assert.NotNull(map.StartPosition);
            Assert.NotNull(map.StairsDownPosition);
        }

        // 観点: Generate_ProducesDeterministicLayout_ForSameSeedAndSize の期待挙動を検証する。
        [Fact]
        public void Generate_ProducesDeterministicLayout_ForSameSeedAndSize()
        {
            var sut = new MapGenerationService();

            var left = sut.Generate(width: 32, height: 32, seed: 777);
            var right = sut.Generate(width: 32, height: 32, seed: 777);

            foreach (var position in left.AllPositions())
            {
                Assert.Equal(left.GetTile(position).Type, right.GetTile(position).Type);
            }

            Assert.Equal(left.StartPosition, right.StartPosition);
            Assert.Equal(left.StairsDownPosition, right.StairsDownPosition);
            Assert.Equal(left.Rooms.Count, right.Rooms.Count);
        }

        // 観点: Generate_PlacesWalkableStartAndStairs_OnDifferentPositions の期待挙動を検証する。
        [Fact]
        public void Generate_PlacesWalkableStartAndStairs_OnDifferentPositions()
        {
            var sut = new MapGenerationService();

            var map = sut.Generate(width: 28, height: 28, seed: 1010);

            Assert.True(map.StartPosition.HasValue);
            Assert.True(map.StairsDownPosition.HasValue);
            Assert.True(map.IsWalkable(map.StartPosition.Value));
            Assert.True(map.IsWalkable(map.StairsDownPosition.Value));
            Assert.NotEqual(map.StartPosition.Value, map.StairsDownPosition.Value);
        }

        // 観点: Generate_ProducesAtLeastOneRoomAndOneWalkableTile の期待挙動を検証する。
        [Fact]
        public void Generate_ProducesAtLeastOneRoomAndOneWalkableTile()
        {
            var sut = new MapGenerationService();

            var map = sut.Generate(width: 20, height: 20, seed: 42);

            Assert.True(map.Rooms.Count >= 1);
            Assert.Contains(map.AllPositions(), map.IsWalkable);
        }
    }
}

