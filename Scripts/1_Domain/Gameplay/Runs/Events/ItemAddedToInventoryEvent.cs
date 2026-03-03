using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// アイテムがインベントリに追加された出来事です。
    /// </summary>
    public sealed class ItemAddedToInventoryEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 拾った人のID。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 追加されたアイテムのID。
        /// </summary>
        public ItemInstanceId ItemId { get; }

        /// <summary>
        /// 追加されたアイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }

        /// <summary>
        /// 拾った場所の位置。
        /// </summary>
        public Position PickupPosition { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ItemAddedToInventoryEvent(ActorId actorId, ItemInstanceId itemId, ItemId itemType, Position pickupPosition)
        {
            ActorId = actorId;
            ItemId = itemId;
            ItemType = itemType;
            PickupPosition = pickupPosition;
        }
    }
}
