using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Xunit;

namespace Roguelike.Tests.Application.RunQuery.Services
{
    /// <summary>
    /// RunAccessCapabilityPolicy の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunAccessCapabilityPolicyTests
    {
        // 観点: TryResolve_ReturnsTrueOnlyInRun_ForSupportedCapabilities の期待挙動を検証する。
        [Theory]
        [InlineData(RunAccessCapability.OpenMenu)]
        [InlineData(RunAccessCapability.OpenInventory)]
        [InlineData(RunAccessCapability.ExplorationInput)]
        public void TryResolve_ReturnsTrueOnlyInRun_ForSupportedCapabilities(RunAccessCapability capability)
        {
            var sut = new RunAccessCapabilityPolicy();

            var inRunResolved = sut.TryResolve(capability, RunPhaseDto.InRun, out var inRunCanUse);
            var pauseResolved = sut.TryResolve(capability, RunPhaseDto.Pause, out var pauseCanUse);

            Assert.True(inRunResolved);
            Assert.True(inRunCanUse);
            Assert.True(pauseResolved);
            Assert.False(pauseCanUse);
        }

        // 観点: TryResolve_ReturnsFalse_ForUnsupportedCapability の期待挙動を検証する。
        [Fact]
        public void TryResolve_ReturnsFalse_ForUnsupportedCapability()
        {
            var sut = new RunAccessCapabilityPolicy();

            var resolved = sut.TryResolve((RunAccessCapability)999, RunPhaseDto.InRun, out var canUse);

            Assert.False(resolved);
            Assert.False(canUse);
        }
    }
}
