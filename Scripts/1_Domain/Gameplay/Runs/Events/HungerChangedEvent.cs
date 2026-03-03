using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 空腹度が増えたり減ったりした出来事です。
    /// </summary>
    public sealed class HungerChangedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 空腹度が変わった人のID。
        /// </summary>
        public ActorId ActorId { get; }
        /// <summary>
        /// どれだけ変わったか（増えたらプラス、減ったらマイナス）。
        /// </summary>
        public float Delta { get; }
        /// <summary>
        /// 変わった後の空腹度。
        /// </summary>
        public float CurrentHunger { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public HungerChangedEvent(ActorId actorId, float delta, float currentHunger)
        {
            ActorId = actorId;
            Delta = delta;
            CurrentHunger = currentHunger;
        }
    }
}


