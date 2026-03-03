using System;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.SpellPreview.UseCases
{
    /// <summary>
    /// EquippedSpellPreviewQueryHandler の仕様を検証するユニットテストです。
    /// 装備中 Spellbook のプレビュー取得に関する正常系・異常系・境界条件を確認します。
    /// </summary>
    public sealed class EquippedSpellPreviewQueryHandlerTests
    {
        [Fact]
        public void Constructor_Throws_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EquippedSpellPreviewQueryHandler(null));
        }

        [Fact]
        public void Handle_ReturnsFailure_WhenQueryIsNull()
        {
            var handler = CreateHandlerWithNoRun();

            var result = handler.Handle(query: null);

            Assert.True(result.IsFailure);
            Assert.Equal("Query is required.", result.ErrorMessage);
        }

        [Fact]
        public void Handle_ReturnsEmptyPreview_WhenNoRunExists()
        {
            var handler = CreateHandlerWithNoRun();

            var result = handler.Handle(new GetEquippedSpellPreviewQuery());

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.CanPreview);
            Assert.Empty(result.Value.PreviewPositions);
        }

        private static EquippedSpellPreviewQueryHandler CreateHandlerWithNoRun()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var service = new SpellPreviewQueryService(
                new InventoryReadModelService(store),
                new SpellTrajectoryService(),
                store);
            return new EquippedSpellPreviewQueryHandler(service);
        }

    }
}
