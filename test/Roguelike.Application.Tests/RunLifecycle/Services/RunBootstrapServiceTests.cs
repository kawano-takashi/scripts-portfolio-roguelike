using System;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunLifecycle.Services
{
    /// <summary>
    /// RunBootstrapService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunBootstrapServiceTests
    {
        // 観点: ResolveStartPosition_UsesMapStartPosition_WhenPresent の期待挙動を検証する。
        [Fact]
        public void ResolveStartPosition_UsesMapStartPosition_WhenPresent()
        {
            var sut = new RunBootstrapService();
            var map = ApplicationTestFactory.CreateMap(start: new Position(3, 4));

            var start = sut.ResolveStartPosition(map);

            Assert.Equal(new Position(3, 4), start);
        }

        // 観点: ResolveStartPosition_FallsBackToFirstWalkablePosition の期待挙動を検証する。
        [Fact]
        public void ResolveStartPosition_FallsBackToFirstWalkablePosition()
        {
            var sut = new RunBootstrapService();
            var map = new Map(4, 4);
            map.SetTileType(new Position(2, 1), TileType.Floor);

            var start = sut.ResolveStartPosition(map);

            Assert.Equal(new Position(2, 1), start);
        }

        // 観点: ResolveStartPosition_Throws_WhenNoWalkableTileExists の期待挙動を検証する。
        [Fact]
        public void ResolveStartPosition_Throws_WhenNoWalkableTileExists()
        {
            var sut = new RunBootstrapService();
            var map = new Map(3, 3);

            Assert.Throws<InvalidOperationException>(() => sut.ResolveStartPosition(map));
        }

        // 観点: ApplyInitialVisibility_DoesNothing_WhenSessionIsNull の期待挙動を検証する。
        [Fact]
        public void ApplyInitialVisibility_DoesNothing_WhenSessionIsNull()
        {
            var sut = new RunBootstrapService();

            sut.ApplyInitialVisibility(session: null);
        }

        // 観点: ApplyInitialVisibility_RevealsPlayerPosition の期待挙動を検証する。
        [Fact]
        public void ApplyInitialVisibility_RevealsPlayerPosition()
        {
            var sut = new RunBootstrapService();
            var map = ApplicationTestFactory.CreateMap(width: 6, height: 6, start: new Position(2, 2));
            var run = ApplicationTestFactory.CreateRunSession(map: map, player: ApplicationTestFactory.CreateActor(position: new Position(2, 2)));

            sut.ApplyInitialVisibility(run);

            Assert.True(run.Map.GetTile(new Position(2, 2)).IsVisible);
        }
    }
}
