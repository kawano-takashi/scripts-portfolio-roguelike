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
    /// 敵が次に何をするか決めるための約束です。
    /// </summary>
    public interface IEnemyDecisionPolicy
    {
        /// <summary>
        /// 敵と現在のラン情報から、次の行動を決めます。
        /// </summary>
        RoguelikeAction Decide(Actor enemy, RunSession session);

        /// <summary>
        /// 敵の1ターンあたりの行動回数を取得します。
        /// 倍速敵は2、鈍足敵は0または1を返します。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="turnNumber">現在のターン番号</param>
        /// <returns>行動回数</returns>
        int GetActionCount(Actor enemy, int turnNumber);

        /// <summary>
        /// 新しいフロアでAIの記憶をリセットします。
        /// </summary>
        /// <param name="seed">ランダムシード</param>
        void ResetMemory(int seed);
    }
}
