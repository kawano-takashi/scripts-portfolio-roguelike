using System;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// SpawnBudgetCalculator の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class SpawnBudgetCalculatorTests
    {
        // 観点: Calculate_AssignsNoMonsterHouseBudget_WhenProfileHasNoMonsterHouse の期待挙動を検証する。
        [Fact]
        public void Calculate_AssignsNoMonsterHouseBudget_WhenProfileHasNoMonsterHouse()
        {
            var sut = new SpawnBudgetCalculator();
            var profile = new FloorProfile(floorNumber: 4, type: FloorProfileType.Normal);

            var budget = sut.Calculate(profile, new System.Random(1234));

            Assert.Equal(0, budget.MonsterHouseEnemyCount);
            Assert.Equal(0, budget.MonsterHouseItemCount);
            Assert.InRange(budget.NormalEnemyCount, 5, 7);
            Assert.InRange(budget.NormalItemCount, 5, 6);
        }

        // 観点: Calculate_AssignsMonsterHouseBudget_WhenProfileHasMonsterHouse の期待挙動を検証する。
        [Fact]
        public void Calculate_AssignsMonsterHouseBudget_WhenProfileHasMonsterHouse()
        {
            var sut = new SpawnBudgetCalculator();
            var profile = new FloorProfile(floorNumber: 7, type: FloorProfileType.MonsterHouse);

            var budget = sut.Calculate(profile, new System.Random(1234));

            Assert.InRange(budget.MonsterHouseEnemyCount, 12, 14);
            Assert.InRange(budget.MonsterHouseItemCount, 12, 15);
        }

        // 観点: Calculate_UsesLateFloorWeights_ForUnknownHigherFloor の期待挙動を検証する。
        [Fact]
        public void Calculate_UsesLateFloorWeights_ForUnknownHigherFloor()
        {
            var sut = new SpawnBudgetCalculator();
            var profile = new FloorProfile(floorNumber: 99, type: FloorProfileType.Normal);

            var budget = sut.Calculate(profile, new System.Random(0));

            Assert.Equal(new[] { 20, 35, 45 }, budget.EnemyWeights);
            Assert.Equal(new[] { 20, 50, 30 }, budget.ItemWeights);
            Assert.Equal(new[] { 15, 45, 30, 10 }, budget.MonsterHouseItemWeights);
        }

        // 観点: Calculate_Throws_WhenRandomIsNull の期待挙動を検証する。
        [Fact]
        public void Calculate_Throws_WhenRandomIsNull()
        {
            var sut = new SpawnBudgetCalculator();
            var profile = new FloorProfile(floorNumber: 4, type: FloorProfileType.Normal);

            Assert.Throws<ArgumentNullException>(() => sut.Calculate(profile, random: null));
        }

        // 観点: SpawnBudget_CopiesWeightArrays_OnConstruction の期待挙動を検証する。
        [Fact]
        public void SpawnBudget_CopiesWeightArrays_OnConstruction()
        {
            var enemyWeights = new[] { 10, 20, 30 };
            var itemWeights = new[] { 40, 50, 60 };
            var monsterHouseItemWeights = new[] { 70, 80, 90, 100 };

            var budget = new SpawnBudget(
                normalEnemyCount: 1,
                normalItemCount: 1,
                foodItemCount: 0,
                monsterHouseEnemyCount: 0,
                monsterHouseItemCount: 0,
                enemyWeights: enemyWeights,
                itemWeights: itemWeights,
                monsterHouseItemWeights: monsterHouseItemWeights);

            enemyWeights[0] = 999;
            itemWeights[1] = 999;
            monsterHouseItemWeights[3] = 999;

            Assert.Equal(10, budget.EnemyWeights[0]);
            Assert.Equal(50, budget.ItemWeights[1]);
            Assert.Equal(100, budget.MonsterHouseItemWeights[3]);
        }
    }
}
