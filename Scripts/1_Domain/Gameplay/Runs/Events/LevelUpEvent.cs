using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// レベルアップした出来事です。
    /// </summary>
    public sealed class LevelUpEvent : IRoguelikeEvent
    {
        /// <summary>
        /// だれがレベルアップしたか。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// 以前のレベル。
        /// </summary>
        public int OldLevel { get; }

        /// <summary>
        /// 新しいレベル。
        /// </summary>
        public int NewLevel { get; }

        /// <summary>
        /// レベルアップ後の最大HP。
        /// </summary>
        public int NewMaxHp { get; }

        /// <summary>
        /// レベルアップ後の攻撃力。
        /// </summary>
        public int NewAttack { get; }

        /// <summary>
        /// レベルアップ後の防御力。
        /// </summary>
        public int NewDefense { get; }

        /// <summary>
        /// レベルアップイベントを作ります。
        /// </summary>
        public LevelUpEvent(ActorId actorId, int oldLevel, int newLevel, int newMaxHp, int newAttack, int newDefense)
        {
            ActorId = actorId;
            OldLevel = oldLevel;
            NewLevel = newLevel;
            NewMaxHp = newMaxHp;
            NewAttack = newAttack;
            NewDefense = newDefense;
        }
    }
}


