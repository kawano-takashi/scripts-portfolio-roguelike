using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// アイテムの配置情報を表す値オブジェクトです。
    /// </summary>
    public readonly struct ItemSpawnEntry
    {
        /// <summary>
        /// 配置位置。
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// アイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }

        /// <summary>
        /// 強化情報。
        /// </summary>
        public ItemEnhancements Enhancements { get; }

        /// <summary>
        /// ItemSpawnEntryを作成します。
        /// </summary>
        public ItemSpawnEntry(Position position, ItemId itemType,
                              ItemEnhancements enhancements = null)
        {
            Position = position;
            ItemType = itemType;
            Enhancements = enhancements ?? ItemEnhancements.None;
        }
    }
}


