using System;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunLifecycle.UseCases
{
    /// <summary>
    /// StartRunCommandHandlerValidation の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class StartRunCommandHandlerValidationTests
    {
        // 観点: Constructor_Throws_WhenDependenciesAreNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenDependenciesAreNull()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var orchestrator = CreateOrchestrator();
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var validator = new StartRunCommandValidator();

            Assert.Throws<ArgumentNullException>(() => new StartRunCommandHandler(null, orchestrator, assembler, validator));
            Assert.Throws<ArgumentNullException>(() => new StartRunCommandHandler(store, null, assembler, validator));
            Assert.Throws<ArgumentNullException>(() => new StartRunCommandHandler(store, orchestrator, null, validator));
            Assert.Throws<ArgumentNullException>(() => new StartRunCommandHandler(store, orchestrator, assembler, null));
        }

        // 観点: Execute_ReturnsValidationFailure_WhenCommandIsInvalid の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsValidationFailure_WhenCommandIsInvalid()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var handler = new StartRunCommandHandler(
                store,
                CreateOrchestrator(),
                new RunExecutionResultAssembler(new RunEventProjector()),
                new StartRunCommandValidator());

            var result = handler.Handle(ApplicationTestFactory.CreateStartRunCommand(floor: 0, clearFloor: 0));

            Assert.True(result.IsFailure);
            Assert.Equal("Validation failed.", result.ErrorMessage);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.False(store.HasRun);
        }

        private static RunSessionOrchestrator CreateOrchestrator()
        {
            return new RunSessionOrchestrator(
                new StubMapGenerationService(),
                new StubPopulationService(),
                new PlayerInitializationPolicy(new PlayerInitializationService()),
                new RunBootstrapService());
        }

        private sealed class StubMapGenerationService : IMapGenerationService
        {
            public Map Generate(int seed)
            {
                return Generate(8, 8, seed);
            }

            public Map Generate(int width, int height, int seed)
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

        private sealed class StubPopulationService : IRunPopulationService
        {
            public void Populate(RunSession session)
            {
            }
        }
    }
}
