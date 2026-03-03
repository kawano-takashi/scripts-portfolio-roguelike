using System;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.SpellPreview.Services
{
    /// <summary>
    /// SpellPreviewQueryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SpellPreviewQueryServiceTests
    {
        // 観点: Constructor_Throws_WhenDependenciesAreNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenDependenciesAreNull()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var inventory = new InventoryReadModelService(store);

            Assert.Throws<ArgumentNullException>(() => new SpellPreviewQueryService(null, new SpellTrajectoryService(), store));
            Assert.Throws<ArgumentNullException>(() => new SpellPreviewQueryService(inventory, null, store));
            Assert.Throws<ArgumentNullException>(() => new SpellPreviewQueryService(inventory, new SpellTrajectoryService(), null));
        }

        // 観点: TryBuildSpellPreview_ReturnsFalse_WhenSpellPreviewContextCannotBeResolved の期待挙動を検証する。
        [Fact]
        public void TryBuildSpellPreview_ReturnsFalse_WhenSpellPreviewContextCannotBeResolved()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            var store = new ApplicationTestFactory.SpyRunStore(run);
            var inventory = new InventoryReadModelService(store);
            var sut = new SpellPreviewQueryService(inventory, new SpellTrajectoryService(), store);

            var found = sut.TryBuildSpellPreview(Guid.NewGuid(), out var positions);

            Assert.False(found);
            Assert.Empty(positions);
        }

        // 観点: TryBuildSpellPreview_ReturnsProjectedPositions_ForSpellbookItem の期待挙動を検証する。
        [Fact]
        public void TryBuildSpellPreview_ReturnsProjectedPositions_ForSpellbookItem()
        {
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(width: 8, height: 8, start: new Position(1, 1)),
                player: ApplicationTestFactory.CreateActor(position: new Position(1, 1)));
            var spellbook = ApplicationTestFactory.AddInventoryItem(run.Player, Roguelike.Domain.Gameplay.Items.Enums.ItemId.SpellbookMagicFire);
            run.Player.Equipment.TryEquip(spellbook, out _, out _);
            var store = new ApplicationTestFactory.SpyRunStore(run);
            var inventory = new InventoryReadModelService(store);
            var sut = new SpellPreviewQueryService(inventory, new SpellTrajectoryService(), store);

            var found = sut.TryBuildSpellPreview(spellbook.Id.Value, out var positions);

            Assert.True(found);
            Assert.NotEmpty(positions);
            Assert.Equal(1, positions[0].X);
            Assert.Equal(2, positions[0].Y);
        }
    }
}
