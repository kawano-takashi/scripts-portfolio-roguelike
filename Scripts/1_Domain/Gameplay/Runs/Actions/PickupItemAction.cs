using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// 足元のアイテムを拾う行動です。
    /// </summary>
    public sealed class PickupItemAction : RoguelikeAction
    {
        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public PickupItemAction(ActorId actorId) : base(actorId)
        {
        }
    }
}


