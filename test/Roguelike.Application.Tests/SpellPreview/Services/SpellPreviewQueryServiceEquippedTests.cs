using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.SpellPreview.Services
{
    /// <summary>
    /// SpellPreviewQueryService の装備 Spellbook プレビュー機能を検証するユニットテストです。
    /// </summary>
    public sealed class SpellPreviewQueryServiceEquippedTests
    {
        [Fact]
        public void TryBuildEquippedSpellPreview_ReturnsFalse_WhenNoRunExists()
        {
            var store = new ApplicationTestFactory.SpyRunStore();
            var sut = new SpellPreviewQueryService(
                new InventoryReadModelService(store),
                new SpellTrajectoryService(),
                store);

            var found = sut.TryBuildEquippedSpellPreview(out var positions);

            Assert.False(found);
            Assert.Empty(positions);
        }

        [Fact]
        public void TryBuildEquippedSpellPreview_ReturnsFalse_WhenNoSpellbookEquipped()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            var store = new ApplicationTestFactory.SpyRunStore(run);
            var sut = new SpellPreviewQueryService(
                new InventoryReadModelService(store),
                new SpellTrajectoryService(),
                store);

            var found = sut.TryBuildEquippedSpellPreview(out var positions);

            Assert.False(found);
            Assert.Empty(positions);
        }

        [Fact]
        public void TryBuildEquippedSpellPreview_ReturnsPositions_WhenSpellbookEquipped()
        {
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(width: 8, height: 8, start: new Position(1, 1)),
                player: ApplicationTestFactory.CreateActor(position: new Position(1, 1)));
            var spellbook = ApplicationTestFactory.AddInventoryItem(run.Player, ItemId.SpellbookMagicFire);
            run.Player.Equipment.TryEquip(spellbook, out _, out _);

            var store = new ApplicationTestFactory.SpyRunStore(run);
            var sut = new SpellPreviewQueryService(
                new InventoryReadModelService(store),
                new SpellTrajectoryService(),
                store);

            var found = sut.TryBuildEquippedSpellPreview(out var positions);

            Assert.True(found);
            Assert.NotEmpty(positions);
            Assert.Equal(1, positions[0].X);
            Assert.Equal(2, positions[0].Y);
        }
    }
}
