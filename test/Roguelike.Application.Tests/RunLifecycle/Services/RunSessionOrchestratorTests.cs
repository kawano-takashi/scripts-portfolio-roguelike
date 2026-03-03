using System;
using Roguelike.Application.Commands;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunLifecycle.Services
{
    /// <summary>
    /// RunSessionOrchestrator の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunSessionOrchestratorTests
    {
        // 観点: Constructor_Throws_WhenAnyDependencyIsNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenAnyDependencyIsNull()
        {
            var mapService = new StubMapGenerationService();
            var population = new StubPopulationService();
            var policy = new PlayerInitializationPolicy(new PlayerInitializationService());
            var bootstrap = new RunBootstrapService();

            Assert.Throws<ArgumentNullException>(() => new RunSessionOrchestrator(null, population, policy, bootstrap));
            Assert.Throws<ArgumentNullException>(() => new RunSessionOrchestrator(mapService, null, policy, bootstrap));
            Assert.Throws<ArgumentNullException>(() => new RunSessionOrchestrator(mapService, population, null, bootstrap));
            Assert.Throws<ArgumentNullException>(() => new RunSessionOrchestrator(mapService, population, policy, null));
        }

        // 観点: CreateInitialSession_UsesCustomSize_WhenWidthAndHeightAreProvided の期待挙動を検証する。
        [Fact]
        public void CreateInitialSession_UsesCustomSize_WhenWidthAndHeightAreProvided()
        {
            var sut = CreateOrchestrator(new StubMapGenerationService());
            var command = ApplicationTestFactory.CreateStartRunCommand(width: 30, height: 25, startImmediately: true);

            var session = sut.CreateInitialSession(command);

            Assert.Equal(30, session.Map.Size.Width);
            Assert.Equal(25, session.Map.Size.Height);
            Assert.Equal(RunPhase.InRun, session.Phase);
        }

        // 観点: CreateInitialSession_AppliesPlayerParametersAndStartPosition の期待挙動を検証する。
        [Fact]
        public void CreateInitialSession_AppliesPlayerParametersAndStartPosition()
        {
            var sut = CreateOrchestrator(new StubMapGenerationService());
            var command = new StartRunCommand(
                PlayerName: "mage",
                Floor: 1,
                ClearFloor: 10,
                Seed: 1234,
                Width: 12,
                Height: 9,
                StartImmediately: true,
                PlayerMaxHp: 31,
                PlayerAttack: 6,
                PlayerDefense: 4,
                PlayerIntelligence: 17,
                PlayerSightRadius: 10,
                PlayerMaxHunger: 120f);

            var session = sut.CreateInitialSession(command);

            Assert.Equal("mage", session.Player.Name);
            Assert.Equal(31, session.Player.Stats.MaxHp);
            Assert.Equal(6, session.Player.Stats.Attack);
            Assert.Equal(4, session.Player.Stats.Defense);
            Assert.Equal(17, session.Player.Stats.Intelligence);
            Assert.Equal(10, session.Player.Stats.SightRadius);
            Assert.Equal(120f, session.Player.Stats.MaxHunger);
            Assert.Equal(session.Map.StartPosition.Value, session.Player.Position);
            Assert.Equal(Direction.Down, session.Player.Facing);
        }

        // 観点: CreateInitialSession_LeavesRunStartPhase_WhenStartImmediatelyIsFalse の期待挙動を検証する。
        [Fact]
        public void CreateInitialSession_LeavesRunStartPhase_WhenStartImmediatelyIsFalse()
        {
            var sut = CreateOrchestrator(new StubMapGenerationService());
            var command = ApplicationTestFactory.CreateStartRunCommand(startImmediately: false);

            var session = sut.CreateInitialSession(command);

            Assert.Equal(RunPhase.RunStart, session.Phase);
        }

        // 観点: BuildNextFloorSession_MarksCurrentRunCleared_WhenAtClearFloor の期待挙動を検証する。
        [Fact]
        public void BuildNextFloorSession_MarksCurrentRunCleared_WhenAtClearFloor()
        {
            var sut = CreateOrchestrator(new StubMapGenerationService());
            var map = ApplicationTestFactory.CreateMap();
            var current = ApplicationTestFactory.CreateRunSession(map: map, floor: 3, clearFloor: 3);

            var next = sut.BuildNextFloorSession(current);

            Assert.Same(current, next);
            Assert.Equal(RunPhase.Clear, current.Phase);
        }

        // 観点: BuildNextFloorSession_CreatesNewSessionWithIncrementedFloor の期待挙動を検証する。
        [Fact]
        public void BuildNextFloorSession_CreatesNewSessionWithIncrementedFloor()
        {
            var sut = CreateOrchestrator(new StubMapGenerationService());
            var map = ApplicationTestFactory.CreateMap(width: 10, height: 10, start: new Position(1, 1));
            var player = ApplicationTestFactory.CreateActor(position: new Position(4, 4), facing: Direction.UpLeft);
            var current = ApplicationTestFactory.CreateRunSession(map: map, player: player, floor: 1, clearFloor: 10);

            var next = sut.BuildNextFloorSession(current);

            Assert.NotSame(current, next);
            Assert.Equal(2, next.Floor);
            Assert.Equal(10, next.Map.Size.Width);
            Assert.Equal(10, next.Map.Size.Height);
            Assert.Equal(next.Map.StartPosition.Value, next.Player.Position);
            Assert.Equal(Direction.Down, next.Player.Facing);
            Assert.Equal(RunPhase.InRun, next.Phase);
        }

        private static RunSessionOrchestrator CreateOrchestrator(StubMapGenerationService mapService)
        {
            return new RunSessionOrchestrator(
                mapService,
                new StubPopulationService(),
                new PlayerInitializationPolicy(new PlayerInitializationService()),
                new RunBootstrapService());
        }

        private sealed class StubPopulationService : IRunPopulationService
        {
            public void Populate(RunSession session)
            {
            }
        }

        private sealed class StubMapGenerationService : IMapGenerationService
        {
            public Map Generate(int seed)
            {
                return CreateMap(MapGenerationService.DefaultWidth, MapGenerationService.DefaultHeight);
            }

            public Map Generate(int width, int height, int seed)
            {
                return CreateMap(width, height);
            }

            private static Map CreateMap(int width, int height)
            {
                var map = new Map(width, height);
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        map.SetTileType(new Position(x, y), TileType.Floor);
                    }
                }

                map.SetStartPosition(new Position(1, 1));
                return map;
            }
        }
    }
}
