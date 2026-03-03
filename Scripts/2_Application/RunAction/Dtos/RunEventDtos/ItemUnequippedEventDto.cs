using System;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    public readonly struct ItemUnequippedEventDto : IRunEventDto
    {
        public RunEventKind Kind => RunEventKind.ItemUnequipped;
        public int TurnNumber { get; }
        public Guid ActorId { get; }
        public Guid ItemId { get; }
        public int ItemTypeValue { get; }
        public int EquipmentSlotValue { get; }

        public ItemUnequippedEventDto(
            int turnNumber,
            Guid actorId,
            Guid itemId,
            int itemTypeValue,
            int equipmentSlotValue)
        {
            TurnNumber = turnNumber;
            ActorId = actorId;
            ItemId = itemId;
            ItemTypeValue = itemTypeValue;
            EquipmentSlotValue = equipmentSlotValue;
        }
    }
}
