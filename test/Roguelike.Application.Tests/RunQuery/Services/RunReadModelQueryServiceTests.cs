using System;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunQuery.Services
{
    /// <summary>
    /// RunReadModelQueryService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunReadModelQueryServiceTests
    {
        // 観点: TryGetCurrentRunReadModel_ReturnsFalse_WhenRunIsMissing の期待挙動を検証する。
        [Fact]
        public void TryGetCurrentRunReadModel_ReturnsFalse_WhenRunIsMissing()
        {
            var sut = new RunReadModelQueryService(new ApplicationTestFactory.SpyRunStore());

            var found = sut.TryGetCurrentRunReadModel(out var readModel);

            Assert.False(found);
            Assert.False(readModel.HasRun);
        }

        // 観点: TryGetCurrentRunReadModel_ProjectsEnemiesItemsAndMap の期待挙動を検証する。
        [Fact]
        public void TryGetCurrentRunReadModel_ProjectsEnemiesItemsAndMap()
        {
            var enemy = ApplicationTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(3, 3), enemyArchetype: EnemyArchetype.Ranged);
            var item = new MapItem(new ItemInstanceId(Guid.NewGuid()), ItemId.HealingPotion, new Position(2, 2));
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(width: 6, height: 6, start: new Position(1, 1), stairs: new Position(4, 4)),
                enemies: new[] { enemy },
                items: new[] { item });
            var sut = new RunReadModelQueryService(new ApplicationTestFactory.SpyRunStore(run));

            var found = sut.TryGetCurrentRunReadModel(out var readModel);

            Assert.True(found);
            Assert.True(readModel.HasRun);
            Assert.Equal(6, readModel.Map.Width);
            Assert.Single(readModel.Enemies);
            Assert.Single(readModel.GroundItems);
            Assert.Equal(enemy.Id.Value, readModel.Enemies[0].ActorId);
            Assert.Equal(item.Id.Value, readModel.GroundItems[0].ItemId);
        }
    }
}
