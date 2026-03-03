using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 「だれがどこへ動いたか」を表す出来事です。
    /// </summary>
    public sealed class ActorMovedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 動いた人のID。
        /// </summary>
        public ActorId ActorId { get; }
        /// <summary>
        /// もとの位置。
        /// </summary>
        public Position From { get; }
        /// <summary>
        /// 移動先の位置。
        /// </summary>
        public Position To { get; }
        /// <summary>
        /// 移動できたかどうか。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ActorMovedEvent(ActorId actorId, Position from, Position to, bool success)
        {
            ActorId = actorId;
            From = from;
            To = to;
            Success = success;
        }
    }
}


