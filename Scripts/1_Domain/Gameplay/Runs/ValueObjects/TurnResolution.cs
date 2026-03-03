using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.ValueObjects
{
    /// <summary>
    /// 1ターンの結果をまとめた箱です。
    /// </summary>
    public readonly struct TurnResolution
    {
        /// <summary>
        /// 行動解決が成功したかどうか。
        /// </summary>
        public bool ActionResolved { get; }
        /// <summary>
        /// この行動でターンが進んだかどうか。
        /// </summary>
        public bool TurnConsumed { get; }
        /// <summary>
        /// 今のターン番号（何ターン目か）。
        /// </summary>
        public int TurnNumber { get; }
        /// <summary>
        /// そのターンで起きた出来事の一覧。
        /// </summary>
        public IReadOnlyList<IRoguelikeEvent> Events { get; }
        /// <summary>
        /// プレイヤー移動アクションの結果。
        /// 移動以外の行動では HasValue が false になります。
        /// </summary>
        public ActorMoveOutcome PlayerMoveOutcome { get; }

        /// <summary>
        /// 結果を作るときの入口です。
        /// </summary>
        public TurnResolution(
            bool turnConsumed,
            int turnNumber,
            IReadOnlyList<IRoguelikeEvent> events,
            bool actionResolved,
            ActorMoveOutcome playerMoveOutcome)
        {
            ActionResolved = actionResolved;
            TurnConsumed = turnConsumed;
            TurnNumber = turnNumber;
            Events = events ?? new List<IRoguelikeEvent>();
            PlayerMoveOutcome = playerMoveOutcome;
        }
    }
}
