using Roguelike.Application.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunQuery.Services
{
    /// <summary>
    /// RunSnapshotQueryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunSnapshotQueryServiceTests
    {
        // 観点: TryGetCurrentRunSnapshot_ReturnsFalse_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void TryGetCurrentRunSnapshot_ReturnsFalse_WhenRunIsMissing()
        {
            var sut = new RunSnapshotQueryService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetCurrentRunSnapshot(out var snapshot);

            Assert.False(found);
            Assert.False(snapshot.HasRun);
        }

        // 観点: TryGetCurrentRunSnapshot_ReturnsProjectedSnapshot_WhenRunExists の期待挙動を検証する。
        [Fact]
        public void TryGetCurrentRunSnapshot_ReturnsProjectedSnapshot_WhenRunExists()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            var sut = new RunSnapshotQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetCurrentRunSnapshot(out var snapshot);

            Assert.True(found);
            Assert.True(snapshot.HasRun);
            Assert.Equal(run.Player.Id.Value, snapshot.PlayerId);
            Assert.Equal(run.Floor, snapshot.Floor);
        }
    }
}
