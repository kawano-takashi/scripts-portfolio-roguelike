using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// インベントリのアイテムを床に落とす行動です。
    /// </summary>
    public sealed class DropItemAction : RoguelikeAction
    {
        /// <summary>
        /// 落とすアイテムのID。
        /// </summary>
        public ItemInstanceId ItemId { get; }

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public DropItemAction(ActorId actorId, ItemInstanceId itemId) : base(actorId)
        {
            ItemId = itemId;
        }
    }
}

