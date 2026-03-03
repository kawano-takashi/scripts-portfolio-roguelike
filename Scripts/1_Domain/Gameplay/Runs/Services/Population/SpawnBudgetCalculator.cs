using System;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// 敵とアイテムの配置予算を計算するサービスです。
    /// トルネコ1ベースの予算テーブルを使用します。
    /// </summary>
    public sealed class SpawnBudgetCalculator
    {
        /// <summary>
        /// フロアの配置予算を計算します。
        /// </summary>
        public SpawnBudget Calculate(FloorProfile profile, Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var floorNumber = profile.FloorNumber;
            var floorData = GetFloorBudgetData(floorNumber);

            var normalEnemyCount = random.Next(floorData.EnemyMin, floorData.EnemyMax + 1);
            var normalItemCount = random.Next(floorData.ItemMin, floorData.ItemMax + 1);
            var foodItemCount = random.Next(floorData.FoodMin, floorData.FoodMax + 1);

            int monsterHouseEnemyCount = 0;
            int monsterHouseItemCount = 0;

            if (profile.HasMonsterHouse)
            {
                monsterHouseEnemyCount = random.Next(floorData.MhEnemyMin, floorData.MhEnemyMax + 1);
                monsterHouseItemCount = random.Next(floorData.MhItemMin, floorData.MhItemMax + 1);
            }

            var enemyWeights = GetEnemyWeights(floorNumber);
            var itemWeights = GetItemWeights(floorNumber);
            var monsterHouseItemWeights = GetMonsterHouseItemWeights(floorNumber);

            return new SpawnBudget(
                normalEnemyCount,
                normalItemCount,
                foodItemCount,
                monsterHouseEnemyCount,
                monsterHouseItemCount,
                enemyWeights,
                itemWeights,
                monsterHouseItemWeights);
        }

        /// <summary>
        /// 階数に応じた予算データを返します。
        /// </summary>
        private static FloorBudgetData GetFloorBudgetData(int floorNumber)
        {
            return floorNumber switch
            {
                1 => new FloorBudgetData(3, 4, 5, 6, 1, 1, 0, 0, 0, 0),
                2 => new FloorBudgetData(4, 5, 5, 6, 1, 1, 0, 0, 0, 0),
                3 => new FloorBudgetData(5, 6, 5, 6, 1, 2, 8, 10, 10, 12),
                4 => new FloorBudgetData(5, 7, 5, 6, 1, 2, 10, 12, 10, 12),
                5 => new FloorBudgetData(6, 7, 5, 7, 1, 2, 10, 12, 12, 14),
                6 => new FloorBudgetData(6, 8, 5, 7, 1, 2, 10, 14, 12, 15),
                7 => new FloorBudgetData(7, 8, 5, 7, 1, 2, 12, 14, 12, 15),
                8 => new FloorBudgetData(7, 9, 5, 7, 1, 2, 12, 15, 12, 15),
                9 => new FloorBudgetData(8, 10, 5, 7, 2, 2, 12, 15, 12, 15),
                10 => new FloorBudgetData(8, 10, 5, 7, 2, 2, 14, 16, 14, 16),
                _ => new FloorBudgetData(8, 10, 5, 7, 2, 2, 14, 16, 14, 16)
            };
        }

        /// <summary>
        /// 階層ごとの予算データ。
        /// </summary>
        private readonly struct FloorBudgetData
        {
            public int EnemyMin { get; }
            public int EnemyMax { get; }
            public int ItemMin { get; }
            public int ItemMax { get; }
            public int FoodMin { get; }
            public int FoodMax { get; }
            public int MhEnemyMin { get; }
            public int MhEnemyMax { get; }
            public int MhItemMin { get; }
            public int MhItemMax { get; }

            public FloorBudgetData(
                int enemyMin, int enemyMax,
                int itemMin, int itemMax,
                int foodMin, int foodMax,
                int mhEnemyMin, int mhEnemyMax,
                int mhItemMin, int mhItemMax)
            {
                EnemyMin = enemyMin;
                EnemyMax = enemyMax;
                ItemMin = itemMin;
                ItemMax = itemMax;
                FoodMin = foodMin;
                FoodMax = foodMax;
                MhEnemyMin = mhEnemyMin;
                MhEnemyMax = mhEnemyMax;
                MhItemMin = mhItemMin;
                MhItemMax = mhItemMax;
            }
        }

        /// <summary>
        /// 階層に応じた敵の出現確率の重みを返します。
        /// 配列順: Melee, Ranged, Disruptor
        /// </summary>
        private static int[] GetEnemyWeights(int floorNumber)
        {
            return floorNumber switch
            {
                <= 2 => new[] { 100, 0, 0 },
                <= 4 => new[] { 70, 25, 5 },
                <= 6 => new[] { 40, 35, 25 },
                <= 8 => new[] { 30, 35, 35 },
                _ => new[] { 20, 35, 45 }
            };
        }

        /// <summary>
        /// 階層に応じたアイテムの出現確率の重みを返します。
        /// 配列順: HealingPotion, Spellbook, Armor
        /// </summary>
        private static int[] GetItemWeights(int floorNumber)
        {
            return floorNumber switch
            {
                <= 2 => new[] { 50, 35, 15 },
                <= 4 => new[] { 40, 45, 15 },
                <= 6 => new[] { 30, 50, 20 },
                <= 8 => new[] { 25, 50, 25 },
                _ => new[] { 20, 50, 30 }
            };
        }

        /// <summary>
        /// モンスターハウス用のアイテム出現確率の重みを返します。
        /// 配列順: HealingPotion, Spellbook, Armor, FoodRation
        /// </summary>
        private static int[] GetMonsterHouseItemWeights(int floorNumber)
        {
            return floorNumber switch
            {
                <= 2 => new[] { 30, 40, 20, 10 },
                <= 4 => new[] { 25, 45, 20, 10 },
                <= 6 => new[] { 20, 45, 25, 10 },
                <= 8 => new[] { 15, 50, 25, 10 },
                _ => new[] { 15, 45, 30, 10 }
            };
        }
    }
}


