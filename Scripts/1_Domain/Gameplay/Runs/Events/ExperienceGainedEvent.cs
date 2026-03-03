using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 経験値を獲得した出来事です。
    /// </summary>
    public sealed class ExperienceGainedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// だれが経験値を獲得したか。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 獲得した経験値の量。
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// 獲得後の現在経験値。
        /// </summary>
        public int CurrentExp { get; }

        /// <summary>
        /// 次のレベルまでに必要な経験値。
        /// </summary>
        public int ExpToNextLevel { get; }

        /// <summary>
        /// 経験値の発生源となった敵のID（任意）。
        /// </summary>
        public ActorId? SourceEnemyId { get; }

        /// <summary>
        /// 経験値獲得イベントを作ります。
        /// </summary>
        public ExperienceGainedEvent(ActorId actorId, int amount, int currentExp, int expToNextLevel, ActorId? sourceEnemyId = null)
        {
            ActorId = actorId;
            Amount = amount;
            CurrentExp = currentExp;
            ExpToNextLevel = expToNextLevel;
            SourceEnemyId = sourceEnemyId;
        }
    }
}


