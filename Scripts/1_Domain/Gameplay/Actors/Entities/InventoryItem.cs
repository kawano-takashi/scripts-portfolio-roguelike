using System;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// インベントリに入っている1つのアイテムです。
    /// </summary>
    public sealed class InventoryItem
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
        /// 強化情報。
        /// </summary>
        public ItemEnhancements Enhancements { get; }

        /// <summary>
        /// インベントリアイテムを作るときの入口です。
        /// </summary>
        public InventoryItem(
            ItemInstanceId id,
            ItemId itemType,
            ItemEnhancements enhancements = null)
        {
            Id = id;
            ItemType = itemType;
            Enhancements = enhancements ?? ItemEnhancements.None;
        }

        /// <summary>
        /// マップ上のアイテムからインベントリアイテムを作ります。
        /// </summary>
        public static InventoryItem FromMapItem(MapItem mapItem)
        {
            if (mapItem == null)
            {
                throw new ArgumentNullException(nameof(mapItem));
            }

            // マップ上の識別子と強化情報をそのまま引き継ぎます。
            return new InventoryItem(mapItem.Id, mapItem.ItemType, mapItem.Enhancements);
        }
    }
}

