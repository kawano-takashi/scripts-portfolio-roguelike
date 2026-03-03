using System.Collections.Generic;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// フロア全体の配置計画を表す値オブジェクトです。
    /// </summary>
    public readonly struct SpawnPlan
    {
        /// <summary>
        /// 敵の配置情報一覧。
        /// </summary>
        public IReadOnlyList<EnemySpawnEntry> Enemies { get; }

        /// <summary>
        /// アイテムの配置情報一覧。
        /// </summary>
        public IReadOnlyList<ItemSpawnEntry> Items { get; }

        /// <summary>
        /// 部屋の役割割り当て一覧。
        /// </summary>
        public IReadOnlyList<RoomAssignment> RoomAssignments { get; }

        /// <summary>
        /// SpawnPlanを作成します。
        /// </summary>
        public SpawnPlan(
            IReadOnlyList<EnemySpawnEntry> enemies,
            IReadOnlyList<ItemSpawnEntry> items,
            IReadOnlyList<RoomAssignment> roomAssignments)
        {
            Enemies = enemies ?? new List<EnemySpawnEntry>();
            Items = items ?? new List<ItemSpawnEntry>();
            RoomAssignments = roomAssignments ?? new List<RoomAssignment>();
        }
    }
}


