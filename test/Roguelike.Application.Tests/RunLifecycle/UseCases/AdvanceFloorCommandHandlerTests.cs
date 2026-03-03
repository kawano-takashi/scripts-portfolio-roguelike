using Xunit;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
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

namespace Roguelike.Tests.Application.RunLifecycle.UseCases
{
    /// <summary>
    /// AdvanceFloorCommandHandler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class AdvanceFloorCommandHandlerTests
    {
        // 観点: Execute_AdvancesFloor_AndSavesNextSession の期待挙動を検証する。
        [Fact]
        public void Execute_AdvancesFloor_AndSavesNextSession()
        {
            // 実行時に次フロアのセッションが生成・保存され、フロア番号が進むことを確認する。
            var current = CreateRunSession(floor: 1);
            var repository = new ApplicationTestFactory.PersistentRunStoreFake(current);
            var handler = new AdvanceFloorCommandHandler(
                repository,
                CreateSessionOrchestrator(),
                new RunExecutionResultAssembler(new RunEventProjector()));

            var result = handler.Handle();

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Advanced);
            Assert.Single(repository.SavedSessions);
            Assert.NotSame(current, repository.Current);
            Assert.Equal(2, repository.Current.Floor);
            Assert.Equal(result.Value.Snapshot.Floor, repository.Current.Floor);
            Assert.Equal(result.Value.Snapshot.PlayerId, repository.Current.Player.Id.Value);
        }

        private static RunSessionOrchestrator CreateSessionOrchestrator()
        {
            return new RunSessionOrchestrator(
                new FixedMapGenerationService(),
                new NoOpRunPopulationService(),
                new PlayerInitializationPolicy(new PlayerInitializationService()),
                new RunBootstrapService());
        }

        private static RunSession CreateRunSession(int floor)
        {
            var player = new Actor(
                ActorId.NewId(),
                "tester",
                Faction.Player,
                new Position(1, 1),
                new ActorStats(maxHp: 20, attack: 3, defense: 1, intelligence: 14, sightRadius: 8, maxHunger: 100f));

            var map = FixedMapGenerationService.CreateMap(width: 8, height: 8);
            var run = new RunSession(
                seed: 1234,
                floor: floor,
                map: map,
                player: player,
                clearFloor: 10);

            run.StartRun();
            return run;
        }

        private sealed class FixedMapGenerationService : IMapGenerationService
        {
            public Map Generate(int seed)
            {
                return CreateMap(MapGenerationService.DefaultWidth, MapGenerationService.DefaultHeight);
            }

            public Map Generate(int width, int height, int seed)
            {
                return CreateMap(width, height);
            }

            public static Map CreateMap(int width, int height)
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

        private sealed class NoOpRunPopulationService : IRunPopulationService
        {
            public void Populate(RunSession session)
            {
            }
        }
    }
}




