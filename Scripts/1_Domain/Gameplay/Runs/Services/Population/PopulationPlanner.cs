using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// 敵とアイテムの配置計画を作成するサービスです。
    /// </summary>
    public sealed class PopulationPlanner
    {
        // 敵の種類の候補
        private static readonly EnemyArchetype[] EnemyTypes =
        {
            EnemyArchetype.Melee,
            EnemyArchetype.Ranged,
            EnemyArchetype.Disruptor
        };

        /// <summary>
        /// 配置計画を作成します。
        /// </summary>
        public SpawnPlan CreatePlan(
            Map map,
            IReadOnlyList<RoomAssignment> roomAssignments,
            SpawnBudget budget,
            Position playerPosition,
            Random random)
        {
            if (map == null || roomAssignments == null || roomAssignments.Count == 0)
            {
                return new SpawnPlan(
                    Array.Empty<EnemySpawnEntry>(),
                    Array.Empty<ItemSpawnEntry>(),
                    roomAssignments ?? Array.Empty<RoomAssignment>());
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var enemies = new List<EnemySpawnEntry>();
            var items = new List<ItemSpawnEntry>();
            var usedPositions = new HashSet<Position>();

            // プレイヤー位置は使用済みとしてマーク
            usedPositions.Add(playerPosition);

            // 階段位置も使用済みとしてマーク
            if (map.StairsDownPosition.HasValue)
            {
                usedPositions.Add(map.StairsDownPosition.Value);
            }

            // 通常部屋の位置を収集
            var normalRoomPositions = new List<Position>();
            RoomAssignment? monsterHouseAssignment = null;

            for (int i = 0; i < roomAssignments.Count; i++)
            {
                var assignment = roomAssignments[i];

                if (assignment.Role == RoomRole.MonsterHouse)
                {
                    monsterHouseAssignment = assignment;
                    continue;
                }

                // スタート部屋には敵を配置しない
                if (assignment.Role == RoomRole.Start)
                {
                    // アイテムは配置可能
                    CollectRoomPositions(assignment.Room, usedPositions, normalRoomPositions);
                    continue;
                }

                CollectRoomPositions(assignment.Room, usedPositions, normalRoomPositions);
            }

            Shuffle(normalRoomPositions, random);

            // 通常部屋に敵を配置
            PlaceNormalEnemies(normalRoomPositions, budget.NormalEnemyCount, enemies, usedPositions, budget.EnemyWeights, random);

            // 食料アイテムを配置
            PlaceFoodItems(normalRoomPositions, budget.FoodItemCount, items, usedPositions, random);

            // 通常アイテムを配置
            var remainingItems = budget.NormalItemCount - budget.FoodItemCount;
            PlaceNormalItems(normalRoomPositions, remainingItems, items, usedPositions, budget.ItemWeights, random);

            // モンスターハウスに敵とアイテムを配置
            if (monsterHouseAssignment.HasValue)
            {
                PlaceMonsterHouseContents(
                    monsterHouseAssignment.Value.Room,
                    budget.MonsterHouseEnemyCount,
                    budget.MonsterHouseItemCount,
                    enemies,
                    items,
                    usedPositions,
                    budget.EnemyWeights,
                    budget.MonsterHouseItemWeights,
                    random);
            }

            return new SpawnPlan(enemies, items, roomAssignments);
        }

        /// <summary>
        /// 部屋内の床位置を収集します。
        /// </summary>
        private static void CollectRoomPositions(MapRect room, HashSet<Position> usedPositions, List<Position> output)
        {
            for (int x = room.Left; x <= room.Right; x++)
            {
                for (int y = room.Top; y <= room.Bottom; y++)
                {
                    var pos = new Position(x, y);
                    if (!usedPositions.Contains(pos))
                    {
                        output.Add(pos);
                    }
                }
            }
        }

        /// <summary>
        /// 通常の敵を配置します。
        /// </summary>
        private static void PlaceNormalEnemies(
            List<Position> positions,
            int count,
            List<EnemySpawnEntry> enemies,
            HashSet<Position> usedPositions,
            IReadOnlyList<int> enemyWeights,
            Random random)
        {
            for (int i = 0; i < count && positions.Count > 0; i++)
            {
                var pos = TakePosition(positions, usedPositions);
                var archetype = SelectWeighted(EnemyTypes, enemyWeights, random);
                enemies.Add(new EnemySpawnEntry(pos, archetype, isInMonsterHouse: false, startsAsleep: false));
            }
        }

        /// <summary>
        /// 食料アイテムを配置します。
        /// </summary>
        private static void PlaceFoodItems(
            List<Position> positions,
            int count,
            List<ItemSpawnEntry> items,
            HashSet<Position> usedPositions,
            Random random)
        {
            for (int i = 0; i < count && positions.Count > 0; i++)
            {
                var pos = TakePosition(positions, usedPositions);
                items.Add(new ItemSpawnEntry(pos, ItemId.FoodRation));
            }
        }

        /// <summary>
        /// 通常アイテムを配置します。
        /// </summary>
        private static void PlaceNormalItems(
            List<Position> positions,
            int count,
            List<ItemSpawnEntry> items,
            HashSet<Position> usedPositions,
            IReadOnlyList<int> itemWeights,
            Random random)
        {
            for (int i = 0; i < count && positions.Count > 0; i++)
            {
                var pos = TakePosition(positions, usedPositions);
                var itemType = SelectNormalItem(itemWeights, random);
                items.Add(new ItemSpawnEntry(pos, itemType));
            }
        }

        /// <summary>
        /// モンスターハウスの内容を配置します。
        /// </summary>
        private static void PlaceMonsterHouseContents(
            MapRect room,
            int enemyCount,
            int itemCount,
            List<EnemySpawnEntry> enemies,
            List<ItemSpawnEntry> items,
            HashSet<Position> usedPositions,
            IReadOnlyList<int> enemyWeights,
            IReadOnlyList<int> monsterHouseItemWeights,
            Random random)
        {
            var positions = new List<Position>();
            CollectRoomPositions(room, usedPositions, positions);
            Shuffle(positions, random);

            // 敵を配置（睡眠状態で開始）
            for (int i = 0; i < enemyCount && positions.Count > 0; i++)
            {
                var pos = TakePosition(positions, usedPositions);
                var archetype = SelectWeighted(EnemyTypes, enemyWeights, random);
                enemies.Add(new EnemySpawnEntry(pos, archetype, isInMonsterHouse: true, startsAsleep: true));
            }

            // アイテムを配置（良いアイテムが出やすい）
            for (int i = 0; i < itemCount && positions.Count > 0; i++)
            {
                var pos = TakePosition(positions, usedPositions);
                var itemType = SelectMonsterHouseItem(monsterHouseItemWeights, random);
                items.Add(new ItemSpawnEntry(pos, itemType));
            }
        }

        /// <summary>
        /// リストから位置を取り出します。
        /// </summary>
        private static Position TakePosition(List<Position> positions, HashSet<Position> usedPositions)
        {
            var pos = positions[positions.Count - 1];
            positions.RemoveAt(positions.Count - 1);
            usedPositions.Add(pos);
            return pos;
        }

        /// <summary>
        /// リストをシャッフルします。
        /// </summary>
        private static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// 重みに基づいてランダムに選択します。
        /// </summary>
        private static T SelectWeighted<T>(T[] items, IReadOnlyList<int> weights, Random random)
        {
            var totalWeight = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                totalWeight += weights[i];
            }

            var roll = random.Next(totalWeight);
            var cumulative = 0;
            for (int i = 0; i < items.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return items[i];
                }
            }

            return items[items.Length - 1];
        }

        private static ItemId SelectNormalItem(IReadOnlyList<int> weights, Random random)
        {
            var index = SelectWeightedIndex(weights, random);
            return index switch
            {
                0 => ItemId.HealingPotion,
                // 旧「Spellbook種別」は、ここで具体的な魔法書ItemIdへ展開します。
                1 => SelectSpellbookItem(random),
                2 => ItemId.Armor,
                _ => ItemId.HealingPotion
            };
        }

        private static ItemId SelectMonsterHouseItem(IReadOnlyList<int> weights, Random random)
        {
            var index = SelectWeightedIndex(weights, random);
            return index switch
            {
                0 => ItemId.HealingPotion,
                // モンスターハウスでも同じく「Spellbook枠」は個別魔法書へ展開します。
                1 => SelectSpellbookItem(random),
                2 => ItemId.Armor,
                3 => ItemId.FoodRation,
                _ => ItemId.FoodRation
            };
        }

        private static int SelectWeightedIndex(IReadOnlyList<int> weights, Random random)
        {
            if (weights == null || weights.Count == 0)
            {
                return 0;
            }

            var totalWeight = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                totalWeight += weights[i];
            }

            if (totalWeight <= 0)
            {
                return 0;
            }

            var roll = random.Next(totalWeight);
            var cumulative = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return i;
                }
            }

            return weights.Count - 1;
        }

        private static ItemId SelectSpellbookItem(Random random)
        {
            var dropPool = ItemCatalog.GetSpellbookDropPool();
            if (dropPool == null || dropPool.Count == 0)
            {
                throw new InvalidOperationException("Spellbook drop pool must not be empty.");
            }

            // DropPool は「魔法書候補をどこまで解禁するか」を調整する単一ポイントです。
            return dropPool[random.Next(dropPool.Count)];
        }

    }
}


