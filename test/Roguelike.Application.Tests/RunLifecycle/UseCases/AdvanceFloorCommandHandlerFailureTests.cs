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
    /// AdvanceFloorCommandHandlerFailure の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class AdvanceFloorCommandHandlerFailureTests
    {
        // 観点: Execute_ReturnsFailure_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenRunIsMissing()
        {
            var handler = new AdvanceFloorCommandHandler(
                new ApplicationTestFactory.SpyRunStore(),
                CreateOrchestrator(),
                new RunExecutionResultAssembler(new RunEventProjector()));

            var result = handler.Handle();

            Assert.True(result.IsFailure);
            Assert.Equal("Active run was not found.", result.ErrorMessage);
        }

        // 観点: Execute_SucceedsAndPreservesHasRun_WhenClearFloorReached の期待挙動を検証する。
        [Fact]
        public void Execute_SucceedsAndPreservesHasRun_WhenClearFloorReached()
        {
            var run = ApplicationTestFactory.CreateRunSession(floor: 3, clearFloor: 3);
            var store = new ApplicationTestFactory.PersistentRunStoreFake(run);
            var handler = new AdvanceFloorCommandHandler(
                store,
                CreateOrchestrator(),
                new RunExecutionResultAssembler(new RunEventProjector()));

            var result = handler.Handle();

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Advanced);
            Assert.True(store.HasRun);
            Assert.Single(store.SavedSessions);
            Assert.Equal(Roguelike.Domain.Gameplay.Runs.Enums.RunPhase.Clear, store.Current.Phase);
            Assert.Equal(Roguelike.Application.Enums.RunPhaseDto.Clear, result.Value.Snapshot.Phase);
        }

        private static RunSessionOrchestrator CreateOrchestrator()
        {
            return new RunSessionOrchestrator(
                new StubMapGenerationService(),
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
    }
}
