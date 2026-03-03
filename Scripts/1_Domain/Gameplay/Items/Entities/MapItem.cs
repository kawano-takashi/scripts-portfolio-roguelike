using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Items.Entities
{
    /// <summary>
    /// マップの上に置かれるアイテムです。
    /// </summary>
    public sealed class MapItem
    {
        /// <summary>
        /// アイテムのID。
        /// </summary>
        public ItemInstanceId Id { get; }
        /// <summary>
        /// アイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }
        /// <summary>
        /// 置いてある位置。
        /// </summary>
        public Position Position { get; }
        /// <summary>
        /// 強化情報。
        /// </summary>
        public ItemEnhancements Enhancements { get; }

        /// <summary>
        /// アイテムを作るときの入口です。
        /// </summary>
        public MapItem(ItemInstanceId id, ItemId itemType, Position position, ItemEnhancements enhancements = null)
        {
            Id = id;
            ItemType = itemType;
            Position = position;
            Enhancements = enhancements ?? ItemEnhancements.None;
        }

        /// <summary>
        /// 新しいIDでアイテムを作ります。
        /// </summary>
        public static MapItem Create(ItemId itemType, Position position, ItemEnhancements enhancements = null)
        {
            return new MapItem(ItemInstanceId.NewId(), itemType, position, enhancements);
        }
    }
}
