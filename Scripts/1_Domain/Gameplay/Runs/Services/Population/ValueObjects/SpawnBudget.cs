using System;
using System.Collections.Generic;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// フロアの配置予算を表す値オブジェクトです。
    /// 「何を」「どれだけ」配置するかの全ルールを保持します。
    /// </summary>
    public readonly struct SpawnBudget
    {
        /// <summary>
        /// 通常部屋に配置する敵の総数。
        /// </summary>
        public int NormalEnemyCount { get; }

        /// <summary>
        /// 通常部屋に配置するアイテムの総数。
        /// </summary>
        public int NormalItemCount { get; }

        /// <summary>
        /// 食料アイテムの数。
        /// </summary>
        public int FoodItemCount { get; }

        /// <summary>
        /// モンスターハウスに配置する敵の数。
        /// </summary>
        public int MonsterHouseEnemyCount { get; }

        /// <summary>
        /// モンスターハウスに配置するアイテムの数。
        /// </summary>
        public int MonsterHouseItemCount { get; }

        /// <summary>
        /// 敵種別の出現確率重み（Melee, Ranged, Disruptor）。
        /// </summary>
        public IReadOnlyList<int> EnemyWeights { get; }

        /// <summary>
        /// アイテム種別の出現確率重み（HealingPotion, Spellbook, Armor）。
        /// </summary>
        public IReadOnlyList<int> ItemWeights { get; }

        /// <summary>
        /// モンスターハウス用アイテム出現確率重み（HealingPotion, Spellbook, Armor, FoodRation）。
        /// </summary>
        public IReadOnlyList<int> MonsterHouseItemWeights { get; }

        /// <summary>
        /// SpawnBudgetを作成します。
        /// </summary>
        public SpawnBudget(
            int normalEnemyCount,
            int normalItemCount,
            int foodItemCount,
            int monsterHouseEnemyCount,
            int monsterHouseItemCount,
            IReadOnlyList<int> enemyWeights,
            IReadOnlyList<int> itemWeights,
            IReadOnlyList<int> monsterHouseItemWeights)
        {
            NormalEnemyCount = normalEnemyCount;
            NormalItemCount = normalItemCount;
            FoodItemCount = foodItemCount;
            MonsterHouseEnemyCount = monsterHouseEnemyCount;
            MonsterHouseItemCount = monsterHouseItemCount;
            EnemyWeights = CopyWeights(enemyWeights, nameof(enemyWeights));
            ItemWeights = CopyWeights(itemWeights, nameof(itemWeights));
            MonsterHouseItemWeights = CopyWeights(monsterHouseItemWeights, nameof(monsterHouseItemWeights));
        }

        private static IReadOnlyList<int> CopyWeights(IReadOnlyList<int> weights, string paramName)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(paramName);
            }

            var copied = new int[weights.Count];
            for (var i = 0; i < weights.Count; i++)
            {
                copied[i] = weights[i];
            }

            return copied;
        }
    }
}

