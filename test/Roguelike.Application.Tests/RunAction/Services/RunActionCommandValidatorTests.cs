using System;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// RunActionCommandValidator の仕様を検証するユニットテストです。
    /// </summary>
    public sealed class RunActionCommandValidatorTests
    {
        private readonly RunActionCommandValidator _validator = new RunActionCommandValidator();

        // 観点: null コマンドが検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenCommandIsNull()
        {
            var result = _validator.Validate(null);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "Command is required.");
        }

        // 観点: 未定義の Direction で MoveRunActionCommand が検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenMoveDirectionIsUndefined()
        {
            var result = _validator.Validate(new MoveRunActionCommand((DirectionDto)999));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "Direction is invalid.");
        }

        // 観点: 未定義の Direction で ChangeFacingRunActionCommand が検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenChangeFacingDirectionIsUndefined()
        {
            var result = _validator.Validate(new ChangeFacingRunActionCommand((DirectionDto)999));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "Direction is invalid.");
        }

        // 観点: 空 Guid の UseItemRunActionCommand が検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenUseItemIdIsEmpty()
        {
            var result = _validator.Validate(new UseItemRunActionCommand(Guid.Empty));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "ItemId is required.");
        }

        // 観点: 空 Guid の DropItemRunActionCommand が検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenDropItemIdIsEmpty()
        {
            var result = _validator.Validate(new DropItemRunActionCommand(Guid.Empty));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "ItemId is required.");
        }

        // 観点: 空 Guid の ToggleEquipItemRunActionCommand が検証エラーになることを検証する。
        [Fact]
        public void Validate_ReturnsInvalid_WhenToggleEquipItemIdIsEmpty()
        {
            var result = _validator.Validate(new ToggleEquipItemRunActionCommand(Guid.Empty));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message == "ItemId is required.");
        }

        // 観点: 有効な MoveRunActionCommand が検証を通過することを検証する。
        [Fact]
        public void Validate_ReturnsValid_ForValidMoveCommand()
        {
            var result = _validator.Validate(new MoveRunActionCommand(DirectionDto.Up));

            Assert.True(result.IsValid);
        }

        // 観点: WaitRunActionCommand が検証を通過することを検証する。
        [Fact]
        public void Validate_ReturnsValid_ForWaitCommand()
        {
            var result = _validator.Validate(new WaitRunActionCommand());

            Assert.True(result.IsValid);
        }

        // 観点: 有効な UseItemRunActionCommand が検証を通過することを検証する。
        [Fact]
        public void Validate_ReturnsValid_ForValidUseItemCommand()
        {
            var result = _validator.Validate(new UseItemRunActionCommand(Guid.NewGuid()));

            Assert.True(result.IsValid);
        }
    }
}
