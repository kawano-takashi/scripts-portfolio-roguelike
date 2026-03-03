using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// 向きを変えるだけの行動です。
    /// </summary>
    public sealed class ChangeFacingAction : RoguelikeAction
    {
        /// <summary>
        /// どの向きを向くか。
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// ターンは消費しません。
        /// </summary>
        public override bool ConsumesTurn => false;

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public ChangeFacingAction(ActorId actorId, Direction direction) : base(actorId)
        {
            Direction = direction;
        }
    }
}
