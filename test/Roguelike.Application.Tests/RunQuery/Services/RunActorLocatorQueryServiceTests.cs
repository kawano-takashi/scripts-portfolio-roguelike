using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunQuery.Services
{
    /// <summary>
    /// RunActorLocatorQueryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunActorLocatorQueryServiceTests
    {
        // 観点: TryGetPlayerId_ReturnsFalse_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void TryGetPlayerId_ReturnsFalse_WhenRunIsMissing()
        {
            var sut = new RunActorLocatorQueryService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetPlayerId(out var playerId);

            Assert.False(found);
            Assert.Equal(System.Guid.Empty, playerId);
        }

        // 観点: IsPlayerActor_ReturnsTrue_OnlyForCurrentPlayerId の期待挙動を検証する。
        [Fact]
        public void IsPlayerActor_ReturnsTrue_OnlyForCurrentPlayerId()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            var sut = new RunActorLocatorQueryService(new ApplicationTestFactory.SpyRunStore(run));

            Assert.True(sut.IsPlayerActor(run.Player.Id.Value));
            Assert.False(sut.IsPlayerActor(System.Guid.Empty));
            Assert.False(sut.IsPlayerActor(System.Guid.NewGuid()));
        }

        // 観点: TryGetStairsDownPosition_ReturnsFalse_WhenStairsAreNotSet の期待挙動を検証する。
        [Fact]
        public void TryGetStairsDownPosition_ReturnsFalse_WhenStairsAreNotSet()
        {
            var map = ApplicationTestFactory.CreateMap(width: 6, height: 6);
            map.SetTileType(new Position(1, 1), Roguelike.Domain.Gameplay.Maps.Enums.TileType.Wall);
            map.SetTileType(new Position(1, 1), Roguelike.Domain.Gameplay.Maps.Enums.TileType.Floor);
            var run = ApplicationTestFactory.CreateRunSession(map: map);
            var sut = new RunActorLocatorQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetStairsDownPosition(out _);

            Assert.False(found);
        }

        // 観点: TryGetStairsDownPosition_ReturnsTrue_WhenStairsExist の期待挙動を検証する。
        [Fact]
        public void TryGetStairsDownPosition_ReturnsTrue_WhenStairsExist()
        {
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(stairs: new Position(5, 5)));
            var sut = new RunActorLocatorQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetStairsDownPosition(out var stairs);

            Assert.True(found);
            Assert.Equal(5, stairs.X);
            Assert.Equal(5, stairs.Y);
        }

        // 観点: TryGetActorPosition_FindsPlayerAndEnemy の期待挙動を検証する。
        [Fact]
        public void TryGetActorPosition_FindsPlayerAndEnemy()
        {
            var enemy = ApplicationTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(4, 4));
            var run = ApplicationTestFactory.CreateRunSession(
                player: ApplicationTestFactory.CreateActor(position: new Position(2, 2)),
                enemies: new[] { enemy });
            var sut = new RunActorLocatorQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var playerFound = sut.TryGetActorPosition(run.Player.Id.Value, out var playerPos);
            var enemyFound = sut.TryGetActorPosition(enemy.Id.Value, out var enemyPos);
            var unknownFound = sut.TryGetActorPosition(System.Guid.NewGuid(), out _);

            Assert.True(playerFound);
            Assert.Equal(2, playerPos.X);
            Assert.Equal(2, playerPos.Y);
            Assert.True(enemyFound);
            Assert.Equal(4, enemyPos.X);
            Assert.Equal(4, enemyPos.Y);
            Assert.False(unknownFound);
        }
    }
}
