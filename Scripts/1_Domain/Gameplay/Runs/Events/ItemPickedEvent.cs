using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// アイテムを拾った出来事です。
    /// </summary>
    public sealed class ItemPickedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 拾った人のID。
        /// </summary>
        public ActorId ActorId { get; }
        /// <summary>
        /// 拾ったアイテムの種類。
        /// </summary>
        public ItemId ItemType { get; }
        /// <summary>
        /// 拾った場所の位置。
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ItemPickedEvent(ActorId actorId, ItemId itemType, Position position)
        {
            ActorId = actorId;
            ItemType = itemType;
            Position = position;
        }
    }
}


