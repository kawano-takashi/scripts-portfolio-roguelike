using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// 敵が何もしないAIです。
    /// テストや仮の実装で使います。
    /// </summary>
    public sealed class NullEnemyDecisionPolicy : IEnemyDecisionPolicy
    {
        /// <summary>
        /// 敵はいつも「待つ」だけにします。
        /// </summary>
        public RoguelikeAction Decide(Actor enemy, RunSession session)
        {
            return new WaitAction(enemy.Id);
        }

        /// <summary>
        /// 常に1を返します。
        /// </summary>
        public int GetActionCount(Actor enemy, int turnNumber)
        {
            return 1;
        }

        /// <summary>
        /// 何もしません。
        /// </summary>
        public void ResetMemory(int seed)
        {
            // NullObject: 何もしない
        }
    }
}
