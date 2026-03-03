using System;
using Xunit;
using Roguelike.Application.Commands;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Infrastructure.RunContext.Repositories;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Application.RunAction.UseCases
{
    /// <summary>
    /// RunActionCommandHandler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunActionCommandHandlerTests
    {
        // 観点: Execute_ReturnsFailure_WhenCommandIsNull の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenCommandIsNull()
        {
            // コマンドが null の場合は実行せず失敗結果を返すことを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(withArmor: false, out _));

            var handler = CreateHandler(repository);

            var result = handler.Handle(command: null);

            Assert.True(result.IsFailure);
        }

        // 観点: Execute_ReturnsFailure_WhenRunDoesNotExist の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenRunDoesNotExist()
        {
            // 現在ランが存在しない場合は失敗結果を返すことを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            var handler = CreateHandler(repository);

            var result = handler.Handle(new WaitRunActionCommand());

            Assert.True(result.IsFailure);
        }

        // 観点: Execute_ReturnsFailure_WhenItemIdIsEmpty の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenItemIdIsEmpty()
        {
            // アイテム ID が空 Guid の場合は入力不正として失敗することを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(withArmor: false, out _));

            var handler = CreateHandler(repository);

            var result = handler.Handle(new UseItemRunActionCommand(Guid.Empty));

            Assert.True(result.IsFailure);
        }

        // 観点: Execute_ToggleEquip_ResolvesAction_WhenEquippableItemExists の期待挙動を検証する。
        [Fact]
        public void Execute_ToggleEquip_ResolvesAction_WhenEquippableItemExists()
        {
            // 装備可能アイテムがある場合は装備切替が解決され、装備イベントが投影されることを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(withArmor: true, out var armorItemId));

            var handler = CreateHandler(repository);

            var result = handler.Handle(new ToggleEquipItemRunActionCommand(armorItemId));

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.TurnResult.ActionResolved);
            Assert.Contains(result.Value.TurnResult.Events, evt => evt is ItemEquippedEventDto);
        }

        private static RunActionCommandHandler CreateHandler(InMemoryRoguelikeRunRepository repository)
        {
            var projector = new RunEventProjector();
            var resultAssembler = new RunExecutionResultAssembler(projector);
            return new RunActionCommandHandler(
                repository,
                new TurnEngine(new NullEnemyDecisionPolicy(), new FieldOfViewService()),
                resultAssembler,
                new RunActionFactory(),
                new RunActionCommandValidator());
        }

        private static RunSession CreateRunSession(bool withArmor, out Guid armorItemId)
        {
            var player = new Actor(
                ActorId.NewId(),
                "tester",
                Faction.Player,
                Position.Zero,
                new ActorStats(maxHp: 20, attack: 3, defense: 1, intelligence: 14, sightRadius: 8, maxHunger: 100));

            armorItemId = Guid.NewGuid();
            if (withArmor)
            {
                var added = player.AddToInventory(new InventoryItem(new ItemInstanceId(armorItemId), ItemId.Armor));
                if (!added)
                {
                    throw new InvalidOperationException("Failed to add armor item for test setup.");
                }
            }

            var run = new RunSession(
                seed: 1234,
                floor: 1,
                map: new Map(8, 8),
                player: player,
                clearFloor: 10);

            run.StartRun();
            return run;
        }
    }
}






