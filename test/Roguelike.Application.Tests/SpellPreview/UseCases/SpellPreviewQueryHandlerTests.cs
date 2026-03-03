using System;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.SpellPreview.UseCases
{
    /// <summary>
    /// SpellPreviewQueryHandler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SpellPreviewQueryHandlerTests
    {
        // 観点: Constructor_Throws_WhenServiceIsNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SpellPreviewQueryHandler(null));
        }

        // 観点: Execute_ReturnsFailure_WhenQueryIsNull の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenQueryIsNull()
        {
            var handler = CreateHandlerWithNoPreview();

            var result = handler.Handle(query: null);

            Assert.True(result.IsFailure);
            Assert.Equal("Query is required.", result.ErrorMessage);
        }

        // 観点: Execute_ReturnsFailure_WhenItemIdIsEmpty の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenItemIdIsEmpty()
        {
            var handler = CreateHandlerWithNoPreview();

            var result = handler.Handle(new GetSpellPreviewQuery(Guid.Empty));

            Assert.True(result.IsFailure);
            Assert.Equal("ItemId is required.", result.ErrorMessage);
        }

        // 観点: Execute_ReturnsFailure_WhenPreviewIsUnavailable の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenPreviewIsUnavailable()
        {
            var handler = CreateHandlerWithNoPreview();

            var result = handler.Handle(new GetSpellPreviewQuery(Guid.NewGuid()));

            Assert.True(result.IsFailure);
            Assert.Equal("Spell preview is unavailable.", result.ErrorMessage);
        }

        private static SpellPreviewQueryHandler CreateHandlerWithNoPreview()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var service = new SpellPreviewQueryService(
                new InventoryReadModelService(store),
                new SpellTrajectoryService(),
                store);
            return new SpellPreviewQueryHandler(service);
        }
    }
}
