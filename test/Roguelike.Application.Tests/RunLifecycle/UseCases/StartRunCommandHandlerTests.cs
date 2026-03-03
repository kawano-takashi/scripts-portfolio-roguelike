using System.Linq;
using Xunit;
using Roguelike.Application.Commands;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Infrastructure.RunContext.Repositories;

namespace Roguelike.Tests.Application.RunLifecycle.UseCases
{
    /// <summary>
    /// StartRunCommandHandler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class StartRunCommandHandlerTests
    {
        // 観点: Execute_SetsInitialMagicFireSpellbookAsEquipped の期待挙動を検証する。
        [Fact]
        public void Execute_SetsInitialMagicFireSpellbookAsEquipped()
        {
            // ラン開始時に初期スペルブックが装備状態で保存されることを確認する。
            var handler = CreateHandler(out var repository);

            var result = handler.Handle(CreateDefaultCommand(seed: 1234));
            var run = RequireCurrentRun(repository);
            var player = run.Player;
            var spellbook = player.Inventory.Items.Single(item => item.ItemType == ItemId.SpellbookMagicFire);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Started);
            Assert.Equal(spellbook.Id, player.Equipment.SpellbookItemId);
        }

        // 観点: Execute_PlayerCanResolveEquippedSpellbook の期待挙動を検証する。
        [Fact]
        public void Execute_PlayerCanResolveEquippedSpellbook()
        {
            // 開始後に装備中スペルブックをインベントリから解決できることを確認する。
            var handler = CreateHandler(out var repository);

            var execution = handler.Handle(CreateDefaultCommand(seed: 1234));
            Assert.True(execution.IsSuccess);

            var run = RequireCurrentRun(repository);
            var found = run.Player.Equipment.TryGetEquippedSpellbook(run.Player.Inventory, out var equippedSpellbook);

            Assert.True(found);
            Assert.NotNull(equippedSpellbook);
            Assert.Equal(ItemId.SpellbookMagicFire, equippedSpellbook.ItemType);
        }

        // 観点: Execute_StillInitializesRunStateNormally の期待挙動を検証する。
        [Fact]
        public void Execute_StillInitializesRunStateNormally()
        {
            // スペルブック初期化の有無に関わらずランの基本状態が正しく初期化されることを確認する。
            var handler = CreateHandler(out var repository);

            var result = handler.Handle(CreateDefaultCommand(seed: 1234));
            var run = RequireCurrentRun(repository);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Started);
            Assert.Equal(RunPhase.InRun, run.Phase);
            Assert.True(repository.TryGetCurrent(out var current));
            Assert.Same(run, current);
            Assert.Equal(new Position(1, 1), run.Player.Position);
        }

        private static RunSession RequireCurrentRun(InMemoryRoguelikeRunRepository repository)
        {
            Assert.True(repository.TryGetCurrent(out var run));
            Assert.NotNull(run);
            return run;
        }

        private static StartRunCommandHandler CreateHandler(out InMemoryRoguelikeRunRepository repository)
        {
            repository = new InMemoryRoguelikeRunRepository();
            var eventProjector = new RunEventProjector();
            var resultAssembler = new RunExecutionResultAssembler(eventProjector);
            var sessionOrchestrator = new RunSessionOrchestrator(
                new FixedMapGenerationService(),
                new NoOpRunPopulationService(),
                new PlayerInitializationPolicy(new PlayerInitializationService()),
                new RunBootstrapService());

            return new StartRunCommandHandler(
                repository,
                sessionOrchestrator,
                resultAssembler,
                new StartRunCommandValidator());
        }

        private static StartRunCommand CreateDefaultCommand(int? seed)
        {
            return new StartRunCommand(
                PlayerName: "tester",
                Floor: 1,
                ClearFloor: 10,
                Seed: seed,
                Width: null,
                Height: null,
                StartImmediately: true,
                PlayerMaxHp: 20,
                PlayerAttack: 3,
                PlayerDefense: 1,
                PlayerIntelligence: 14,
                PlayerSightRadius: 8,
                PlayerMaxHunger: 100f);
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

        private sealed class NoOpRunPopulationService : IRunPopulationService
        {
            public void Populate(RunSession session)
            {
            }
        }
    }
}



