using System;
using System.Linq;
using Xunit;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Infrastructure.RunContext.Repositories;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Application.Inventory.Services
{
    /// <summary>
    /// InventoryReadModelService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class InventoryReadModelServiceTests
    {
        // 観点: TryGetInventoryItems_ProjectsInventoryDtos の期待挙動を検証する。
        [Fact]
        public void TryGetInventoryItems_ProjectsInventoryDtos()
        {
            // インベントリ項目が DTO へ正しく投影され、派生フラグも整合することを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(out var spellbookId, out var armorId));
            var service = new InventoryReadModelService(repository);

            var found = service.TryGetInventoryItems(out var items);

            Assert.True(found);
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);

            var spellbook = items.Single(item => item.ItemId == spellbookId);
            Assert.True(spellbook.IsSpellbook);
            Assert.True(spellbook.CanShowSpellPreview);

            var armor = items.Single(item => item.ItemId == armorId);
            Assert.True(armor.IsEquippable);
            Assert.True(armor.IsEquipped);
            Assert.True(armor.CanToggleEquip);
        }

        // 観点: TryGetSpellPreviewContext_ReturnsFalse_WhenItemHasNoSpellRange の期待挙動を検証する。
        [Fact]
        public void TryGetSpellPreviewContext_ReturnsFalse_WhenItemHasNoSpellRange()
        {
            // 呪文射程を持たないアイテムではプレビュー文脈の取得に失敗することを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(out _, out var armorId));
            var service = new InventoryReadModelService(repository);

            var found = service.TryGetSpellPreviewContext(armorId, out _);

            Assert.False(found);
        }

        // 観点: TryGetSpellPreviewContext_ReturnsContext_ForSpellbookItem の期待挙動を検証する。
        [Fact]
        public void TryGetSpellPreviewContext_ReturnsContext_ForSpellbookItem()
        {
            // スペルブックならプレビュー文脈を取得でき、原点・向き・射程が返ることを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(out var spellbookId, out _));
            var service = new InventoryReadModelService(repository);

            var found = service.TryGetSpellPreviewContext(spellbookId, out var context);

            Assert.True(found);
            Assert.NotNull(context.Map);
            Assert.Equal(new Position(1, 1), context.Origin);
            Assert.Equal(Direction.Down, context.Facing);
            Assert.True(context.Range >= 1);
        }

        // 観点: SpellPreviewQueryService_UsesInventoryReadModel_ForTrajectory の期待挙動を検証する。
        [Fact]
        public void SpellPreviewQueryService_UsesInventoryReadModel_ForTrajectory()
        {
            // プレビュークエリが InventoryReadModel 経由で軌道座標を構築できることを確認する。
            var repository = new InMemoryRoguelikeRunRepository();
            repository.Save(CreateRunSession(out var spellbookId, out _));
            var inventoryReadModel = new InventoryReadModelService(repository);
            var service = new SpellPreviewQueryService(inventoryReadModel, new SpellTrajectoryService(), repository);

            var found = service.TryBuildSpellPreview(spellbookId, out var positions);

            Assert.True(found);
            Assert.NotNull(positions);
            Assert.True(positions.Count > 0);
        }

        private static RunSession CreateRunSession(out Guid spellbookId, out Guid armorId)
        {
            var player = new Actor(
                ActorId.NewId(),
                "tester",
                Faction.Player,
                new Position(1, 1),
                new ActorStats(maxHp: 20, attack: 3, defense: 1, intelligence: 14, sightRadius: 8, maxHunger: 100));

            spellbookId = Guid.NewGuid();
            armorId = Guid.NewGuid();

            var spellbook = new InventoryItem(new ItemInstanceId(spellbookId), ItemId.SpellbookMagicFire);
            var armor = new InventoryItem(new ItemInstanceId(armorId), ItemId.Armor);

            if (!player.AddToInventory(spellbook) || !player.AddToInventory(armor))
            {
                throw new InvalidOperationException("Failed to add test items to inventory.");
            }

            if (!player.Equipment.TryEquip(armor, out _, out _))
            {
                throw new InvalidOperationException("Failed to equip test armor.");
            }

            var map = new Map(8, 8);
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    map.SetTileType(new Position(x, y), TileType.Floor);
                }
            }

            var run = new RunSession(
                seed: 1234,
                floor: 1,
                map: map,
                player: player,
                clearFloor: 10);
            run.StartRun();
            return run;
        }
    }
}








