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
    /// 探索（サーチ）する行動です。
    /// </summary>
    public sealed class SearchAction : RoguelikeAction
    {
        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public SearchAction(ActorId actorId) : base(actorId)
        {
        }
    }
}
