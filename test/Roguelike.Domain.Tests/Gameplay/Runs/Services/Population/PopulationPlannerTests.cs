using System.Collections.Generic;
using System.Linq;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// PopulationPlanner の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class PopulationPlannerTests
    {
        // 観点: CreatePlan_ReturnsEmpty_WhenRoomAssignmentsAreMissing の期待挙動を検証する。
        [Fact]
        public void CreatePlan_ReturnsEmpty_WhenRoomAssignmentsAreMissing()
        {
            var sut = new PopulationPlanner();
            var map = DomainTestFactory.CreateMap();
            var budget = CreateBudget();

            var plan = sut.CreatePlan(map, roomAssignments: new List<RoomAssignment>(), budget, playerPosition: new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(1, 1), random: new System.Random(1));

            Assert.Empty(plan.Enemies);
            Assert.Empty(plan.Items);
        }

        // 観点: CreatePlan_DoesNotUsePlayerOrStairsPosition_AndAvoidsDuplicatePlacements の期待挙動を検証する。
        [Fact]
        public void CreatePlan_DoesNotUsePlayerOrStairsPosition_AndAvoidsDuplicatePlacements()
        {
            var sut = new PopulationPlanner();
            var room = new Roguelike.Domain.Gameplay.Maps.ValueObjects.MapRect(1, 1, 6, 6);
            var map = DomainTestFactory.CreateMap(
                width: 10,
                height: 10,
                floorTiles: AllFloor(room),
                rooms: new[] { room },
                stairs: new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(6, 6));

            var assignments = new[] { new RoomAssignment(room, RoomRole.Normal) };
            var budget = new SpawnBudget(
                normalEnemyCount: 4,
                normalItemCount: 4,
                foodItemCount: 1,
                monsterHouseEnemyCount: 0,
                monsterHouseItemCount: 0,
                enemyWeights: new[] { 100, 0, 0 },
                itemWeights: new[] { 100, 0, 0 },
                monsterHouseItemWeights: new[] { 0, 0, 0, 100 });

            var playerPosition = new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(2, 2);
            var plan = sut.CreatePlan(map, assignments, budget, playerPosition, new System.Random(42));

            var used = new HashSet<Roguelike.Domain.Gameplay.Maps.ValueObjects.Position>();
            used.Add(playerPosition);
            used.Add(map.StairsDownPosition.Value);

            foreach (var enemy in plan.Enemies)
            {
                Assert.DoesNotContain(enemy.Position, used);
                Assert.True(used.Add(enemy.Position));
            }

            foreach (var item in plan.Items)
            {
                Assert.DoesNotContain(item.Position, used);
                Assert.True(used.Add(item.Position));
            }
        }

        // 観点: CreatePlan_PlacesMonsterHouseEntriesWithSleepFlag_WhenMonsterHouseRoomExists の期待挙動を検証する。
        [Fact]
        public void CreatePlan_PlacesMonsterHouseEntriesWithSleepFlag_WhenMonsterHouseRoomExists()
        {
            var sut = new PopulationPlanner();
            var normalRoom = new Roguelike.Domain.Gameplay.Maps.ValueObjects.MapRect(1, 1, 4, 4);
            var monsterHouseRoom = new Roguelike.Domain.Gameplay.Maps.ValueObjects.MapRect(6, 1, 4, 4);
            var map = DomainTestFactory.CreateMap(
                width: 12,
                height: 8,
                floorTiles: AllFloor(normalRoom).Concat(AllFloor(monsterHouseRoom)),
                rooms: new[] { normalRoom, monsterHouseRoom });

            var assignments = new[]
            {
                new RoomAssignment(normalRoom, RoomRole.Normal),
                new RoomAssignment(monsterHouseRoom, RoomRole.MonsterHouse)
            };
            var budget = new SpawnBudget(
                normalEnemyCount: 0,
                normalItemCount: 0,
                foodItemCount: 0,
                monsterHouseEnemyCount: 3,
                monsterHouseItemCount: 2,
                enemyWeights: new[] { 100, 0, 0 },
                itemWeights: new[] { 100, 0, 0 },
                monsterHouseItemWeights: new[] { 0, 0, 0, 100 });

            var plan = sut.CreatePlan(
                map,
                assignments,
                budget,
                playerPosition: new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(2, 2),
                random: new System.Random(7));

            Assert.Equal(3, plan.Enemies.Count);
            Assert.Equal(2, plan.Items.Count);
            Assert.All(plan.Enemies, enemy =>
            {
                Assert.True(enemy.IsInMonsterHouse);
                Assert.True(enemy.StartsAsleep);
                Assert.True(monsterHouseRoom.Contains(enemy.Position));
            });
            Assert.All(plan.Items, item =>
            {
                Assert.Equal(ItemId.FoodRation, item.ItemType);
                Assert.True(monsterHouseRoom.Contains(item.Position));
            });
        }

        // 観点: CreatePlan_Throws_WhenRandomIsNull の期待挙動を検証する。
        [Fact]
        public void CreatePlan_Throws_WhenRandomIsNull()
        {
            var sut = new PopulationPlanner();
            var room = new Roguelike.Domain.Gameplay.Maps.ValueObjects.MapRect(1, 1, 4, 4);
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: AllFloor(room),
                rooms: new[] { room });
            var assignments = new[] { new RoomAssignment(room, RoomRole.Normal) };
            var budget = CreateBudget();

            Assert.Throws<System.ArgumentNullException>(() =>
                sut.CreatePlan(
                    map,
                    assignments,
                    budget,
                    playerPosition: new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(2, 2),
                    random: null));
        }

        private static IEnumerable<Roguelike.Domain.Gameplay.Maps.ValueObjects.Position> AllFloor(Roguelike.Domain.Gameplay.Maps.ValueObjects.MapRect room)
        {
            for (var x = room.Left; x <= room.Right; x++)
            {
                for (var y = room.Top; y <= room.Bottom; y++)
                {
                    yield return new Roguelike.Domain.Gameplay.Maps.ValueObjects.Position(x, y);
                }
            }
        }

        private static SpawnBudget CreateBudget()
        {
            return new SpawnBudget(
                normalEnemyCount: 1,
                normalItemCount: 1,
                foodItemCount: 0,
                monsterHouseEnemyCount: 0,
                monsterHouseItemCount: 0,
                enemyWeights: new[] { 100, 0, 0 },
                itemWeights: new[] { 100, 0, 0 },
                monsterHouseItemWeights: new[] { 0, 0, 0, 100 });
        }
    }
}
