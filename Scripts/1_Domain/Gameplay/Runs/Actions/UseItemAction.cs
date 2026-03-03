using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// インベントリのアイテムを使う行動です。
    /// </summary>
    public sealed class UseItemAction : RoguelikeAction
    {
        /// <summary>
        /// 使うアイテムのID。
        /// </summary>
        public ItemInstanceId ItemId { get; }

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public UseItemAction(ActorId actorId, ItemInstanceId itemId) : base(actorId)
        {
            ItemId = itemId;
        }
    }
}

