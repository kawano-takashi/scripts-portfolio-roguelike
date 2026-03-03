using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    public sealed class ItemEquippedEvent : IRoguelikeEvent
    {
        public ActorId ActorId { get; }
        public ItemInstanceId ItemId { get; }
        public ItemId ItemType { get; }
        public EquipmentSlot Slot { get; }

        public ItemEquippedEvent(ActorId actorId, ItemInstanceId itemId, ItemId itemType, EquipmentSlot slot)
        {
            ActorId = actorId;
            ItemId = itemId;
            ItemType = itemType;
            Slot = slot;
        }
    }
}
