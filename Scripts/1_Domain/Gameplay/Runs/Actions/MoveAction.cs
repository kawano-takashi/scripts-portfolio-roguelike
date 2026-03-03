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
    /// 移動したいという行動です。
    /// </summary>
    public sealed class MoveAction : RoguelikeAction
    {
        /// <summary>
        /// どの向きに動くか。
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public MoveAction(ActorId actorId, Direction direction) : base(actorId)
        {
            Direction = direction;
        }
    }
}
