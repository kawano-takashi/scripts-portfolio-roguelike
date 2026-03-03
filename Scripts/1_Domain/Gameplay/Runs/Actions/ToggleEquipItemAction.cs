using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    public sealed class ToggleEquipItemAction : RoguelikeAction
    {
        public ItemInstanceId ItemId { get; }

        public override bool ConsumesTurn => false;

        public ToggleEquipItemAction(ActorId actorId, ItemInstanceId itemId) : base(actorId)
        {
            ItemId = itemId;
        }
    }
}

