using Roguelike.Application.Commands;
using Roguelike.Application.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunLifecycle.Services
{
    /// <summary>
    /// StartRunCommandValidator の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class StartRunCommandValidatorTests
    {
        // 観点: Validate_ReturnsError_WhenCommandIsNull の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsError_WhenCommandIsNull()
        {
            var sut = new StartRunCommandValidator();

            var result = sut.Validate(command: null);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == "command");
        }

        // 観点: Validate_ReturnsErrors_ForInvalidFloorAndClearFloor の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsErrors_ForInvalidFloorAndClearFloor()
        {
            var sut = new StartRunCommandValidator();
            var command = ApplicationTestFactory.CreateStartRunCommand(floor: 3, clearFloor: 2);

            var result = sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.ClearFloor));
        }

        // 観点: Validate_ReturnsError_WhenOnlyOneMapDimensionIsProvided の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsError_WhenOnlyOneMapDimensionIsProvided()
        {
            var sut = new StartRunCommandValidator();
            var command = ApplicationTestFactory.CreateStartRunCommand(width: 30, height: null);

            var result = sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == "MapSize");
        }

        // 観点: Validate_ReturnsErrors_ForInvalidPlayerParameters の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsErrors_ForInvalidPlayerParameters()
        {
            var sut = new StartRunCommandValidator();
            var command = new StartRunCommand(
                PlayerName: "test",
                Floor: 1,
                ClearFloor: 10,
                Seed: 1,
                Width: null,
                Height: null,
                StartImmediately: true,
                PlayerMaxHp: 0,
                PlayerAttack: -1,
                PlayerDefense: -1,
                PlayerIntelligence: -1,
                PlayerSightRadius: 0,
                PlayerMaxHunger: 0f);

            var result = sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerMaxHp));
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerAttack));
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerDefense));
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerIntelligence));
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerSightRadius));
            Assert.Contains(result.Errors, e => e.Field == nameof(StartRunCommand.PlayerMaxHunger));
        }

        // 観点: Validate_ReturnsValid_ForProperCommand の期待挙動を検証する。
        [Fact]
        public void Validate_ReturnsValid_ForProperCommand()
        {
            var sut = new StartRunCommandValidator();
            var command = ApplicationTestFactory.CreateStartRunCommand(floor: 1, clearFloor: 10, width: 32, height: 32);

            var result = sut.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
