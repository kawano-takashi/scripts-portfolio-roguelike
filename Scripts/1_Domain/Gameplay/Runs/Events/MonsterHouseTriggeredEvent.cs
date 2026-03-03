using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// モンスターハウスが発動したことを表すイベントです。
    /// </summary>
    public sealed class MonsterHouseTriggeredEvent : IRoguelikeEvent
    {
        /// <summary>
        /// モンスターハウスの部屋。
        /// </summary>
        public MapRect Room { get; }

        /// <summary>
        /// 起床した敵の数。
        /// </summary>
        public int AwakenedEnemyCount { get; }

        /// <summary>
        /// MonsterHouseTriggeredEventを作成します。
        /// </summary>
        public MonsterHouseTriggeredEvent(MapRect room, int awakenedEnemyCount)
        {
            Room = room;
            AwakenedEnemyCount = awakenedEnemyCount;
        }
    }
}


