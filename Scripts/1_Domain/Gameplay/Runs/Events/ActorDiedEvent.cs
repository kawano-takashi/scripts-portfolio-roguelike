using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 死亡した出来事です。
    /// </summary>
    public sealed class ActorDiedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 死亡した人のID。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ActorDiedEvent(ActorId actorId)
        {
            ActorId = actorId;
        }
    }
}


