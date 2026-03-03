using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// インベントリのアイテムを床に落とした出来事です。
    /// </summary>
    public sealed class ItemDroppedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 落とした人のID。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 落としたアイテムのID。
        /// </summary>
        public ItemInstanceId ItemId { get; }

        /// <summary>
        /// 落としたアイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }

        /// <summary>
        /// 落とした場所の位置。
        /// </summary>
        public Position DropPosition { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ItemDroppedEvent(ActorId actorId, ItemInstanceId itemId, ItemId itemType, Position dropPosition)
        {
            ActorId = actorId;
            ItemId = itemId;
            ItemType = itemType;
            DropPosition = dropPosition;
        }
    }
}
