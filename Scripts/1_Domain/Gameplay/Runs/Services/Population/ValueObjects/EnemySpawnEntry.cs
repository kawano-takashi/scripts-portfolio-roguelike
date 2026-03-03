using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// 敵の配置情報を表す値オブジェクトです。
    /// </summary>
    public readonly struct EnemySpawnEntry
    {
        /// <summary>
        /// 配置位置。
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// 敵の種類。
        /// </summary>
        public EnemyArchetype Archetype { get; }

        /// <summary>
        /// モンスターハウス内の敵かどうか。
        /// </summary>
        public bool IsInMonsterHouse { get; }

        /// <summary>
        /// 睡眠状態で開始するかどうか。
        /// </summary>
        public bool StartsAsleep { get; }

        /// <summary>
        /// EnemySpawnEntryを作成します。
        /// </summary>
        public EnemySpawnEntry(Position position, EnemyArchetype archetype, bool isInMonsterHouse, bool startsAsleep)
        {
            Position = position;
            Archetype = archetype;
            IsInMonsterHouse = isInMonsterHouse;
            StartsAsleep = startsAsleep;
        }
    }
}


