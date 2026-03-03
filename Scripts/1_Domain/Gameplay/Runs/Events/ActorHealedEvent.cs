using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 回復した出来事です。
    /// </summary>
    public sealed class ActorHealedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 回復した人のID。
        /// </summary>
        public ActorId ActorId { get; }
        /// <summary>
        /// 回復した量。
        /// </summary>
        public int Amount { get; }
        /// <summary>
        /// 回復後のHP。
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ActorHealedEvent(ActorId actorId, int amount, int currentHp)
        {
            ActorId = actorId;
            Amount = amount;
            CurrentHp = currentHp;
        }
    }
}


