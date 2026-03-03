using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// DashStepCommandValidator の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class DashStepCommandValidatorTests
    {
        // 観点: Validate_ReturnsError_WhenCommandIsNull の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsError_WhenCommandIsNull()
        {
            var sut = new DashStepCommandValidator();

            var result = sut.Validate(command: null);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == "command");
        }

        // 観点: Validate_ReturnsError_WhenDirectionIsInvalid の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsError_WhenDirectionIsInvalid()
        {
            var sut = new DashStepCommandValidator();
            var command = new DashStepCommand((DirectionDto)999);

            var result = sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == nameof(DashStepCommand.RequestedDirection));
        }

        // 観点: Validate_ReturnsValid_WhenDirectionIsDefined の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsValid_WhenDirectionIsDefined()
        {
            var sut = new DashStepCommandValidator();
            var command = new DashStepCommand(DirectionDto.DownLeft);

            var result = sut.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
