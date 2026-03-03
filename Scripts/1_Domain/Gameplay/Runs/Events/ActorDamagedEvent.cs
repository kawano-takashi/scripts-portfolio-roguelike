using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// ダメージを受けた出来事です。
    /// </summary>
    public sealed class ActorDamagedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// だれが攻撃したか（わからないときは null）。
        /// </summary>
        public ActorId? SourceId { get; }
        /// <summary>
        /// だれがダメージを受けたか。
        /// </summary>
        public ActorId TargetId { get; }
        /// <summary>
        /// いくつダメージを受けたか。
        /// </summary>
        public int Amount { get; }
        /// <summary>
        /// ダメージのあとに残ったHP。
        /// </summary>
        public int RemainingHp { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public ActorDamagedEvent(ActorId? sourceId, ActorId targetId, int amount, int remainingHp)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Amount = amount;
            RemainingHp = remainingHp;
        }
    }
}


