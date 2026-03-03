using System;
using Xunit;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// RunActionFactory の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunActionFactoryTests
    {
        private RunActionFactory _factory;

        public RunActionFactoryTests()
        {
            _factory = new RunActionFactory();
        }

        // 観点: TryCreate_MoveCommand_ReturnsMoveAction の期待挙動を検証する。
        [Fact]
        public void TryCreate_MoveCommand_ReturnsMoveAction()
        {
            // 移動コマンドが MoveAction に変換され、ActorId と方向が引き継がれることを確認する。
            var actorId = ActorId.NewId();
            var command = new MoveRunActionCommand(DirectionDto.Left);

            var translated = _factory.TryCreate(command, actorId, out var action);

            Assert.True(translated);
            var move = Assert.IsType<MoveAction>(action);
            Assert.Equal(actorId, move.ActorId);
            Assert.Equal(Direction.Left, move.Direction);
        }

        // 観点: TryCreate_UseItemWithoutItemId_ReturnsFalse の期待挙動を検証する。
        [Fact]
        public void TryCreate_UseItemWithoutItemId_ReturnsFalse()
        {
            // ItemId が未指定の UseItem コマンドは変換に失敗することを確認する。
            var command = new UseItemRunActionCommand(Guid.Empty);

            var translated = _factory.TryCreate(command, ActorId.NewId(), out var action);

            Assert.False(translated);
            Assert.Null(action);
        }

        // 観点: TryCreate_InvalidDirection_ReturnsFalse の期待挙動を検証する。
        [Fact]
        public void TryCreate_InvalidDirection_ReturnsFalse()
        {
            // 未定義の方向値を持つ移動コマンドは変換に失敗することを確認する。
            var command = new MoveRunActionCommand((DirectionDto)999);

            var translated = _factory.TryCreate(command, ActorId.NewId(), out var action);

            Assert.False(translated);
            Assert.Null(action);
        }

        // 観点: TryCreate_NullCommand_ReturnsFalse の期待挙動を検証する。
        [Fact]
        public void TryCreate_NullCommand_ReturnsFalse()
        {
            // null コマンドは変換できず false と null action を返すことを確認する。
            var translated = _factory.TryCreate(command: null, ActorId.NewId(), out var action);

            Assert.False(translated);
            Assert.Null(action);
        }
    }
}


