using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// Facing direction change event.
    /// </summary>
    public sealed class ActorFacingChangedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 動いた人のID。
        /// </summary>
        public ActorId ActorId { get; }
        /// <summary>
        /// 新しい向き。
        /// </summary>
        public Direction Facing { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ActorFacingChangedEvent(ActorId actorId, Direction facing)
        {
            ActorId = actorId;
            Facing = facing;
        }
    }
}


