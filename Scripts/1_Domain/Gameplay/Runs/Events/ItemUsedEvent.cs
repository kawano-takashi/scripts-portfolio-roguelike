using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// インベントリのアイテムを使った出来事です。
    /// </summary>
    public sealed class ItemUsedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 使った人のID。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 使ったアイテムのID。
        /// </summary>
        public ItemInstanceId ItemId { get; }

        /// <summary>
        /// 使ったアイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ItemUsedEvent(ActorId actorId, ItemInstanceId itemId, ItemId itemType)
        {
            ActorId = actorId;
            ItemId = itemId;
            ItemType = itemType;
        }
    }
}
