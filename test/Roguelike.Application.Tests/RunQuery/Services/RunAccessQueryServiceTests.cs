using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunQuery.Services
{
    /// <summary>
    /// RunAccessQueryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunAccessQueryServiceTests
    {
        // 観点: HasActiveRun_ReflectsRepositoryState の期待挙動を検証する。
        [Fact]
        public void HasActiveRun_ReflectsRepositoryState()
        {
            var withRun = new RunAccessQueryService(new ApplicationTestFactory.SpyRunStore(ApplicationTestFactory.CreateRunSession()));
            var withoutRun = new RunAccessQueryService(new ApplicationTestFactory.SpyRunStore());

            Assert.True(withRun.HasActiveRun());
            Assert.False(withoutRun.HasActiveRun());
        }

        // 観点: TryGetCurrentRunPhase_ReturnsFalse_WhenRunDoesNotExist の期待挙動を検証する。
        [Fact]
        public void TryGetCurrentRunPhase_ReturnsFalse_WhenRunDoesNotExist()
        {
            var sut = new RunAccessQueryService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetCurrentRunPhase(out var phase);

            Assert.False(found);
            Assert.Equal(RunPhaseDto.None, phase);
        }

        // 観点: TryGetCurrentRunPhase_MapsDomainPhaseToDto の期待挙動を検証する。
        [Theory]
        [InlineData(RunPhase.RunStart, RunPhaseDto.RunStart)]
        [InlineData(RunPhase.InRun, RunPhaseDto.InRun)]
        [InlineData(RunPhase.Pause, RunPhaseDto.Pause)]
        [InlineData(RunPhase.Clear, RunPhaseDto.Clear)]
        [InlineData(RunPhase.GameOver, RunPhaseDto.GameOver)]
        public void TryGetCurrentRunPhase_MapsDomainPhaseToDto(RunPhase domain, RunPhaseDto expected)
        {
            var run = ApplicationTestFactory.CreateRunSession(phase: domain);
            var sut = new RunAccessQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetCurrentRunPhase(out var phase);

            Assert.True(found);
            Assert.Equal(expected, phase);
        }
    }
}
