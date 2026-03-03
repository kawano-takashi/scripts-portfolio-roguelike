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
    /// 「だれが何をしたいか」を表す行動の親クラスです。
    /// 具体的な行動は、このクラスを継承して作ります。
    /// </summary>
    public abstract class RoguelikeAction
    {
        /// <summary>
        /// 行動をする人（キャラクター）のIDです。
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// この行動がターンを消費するかどうか。
        /// </summary>
        public virtual bool ConsumesTurn => true;

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        protected RoguelikeAction(ActorId actorId)
        {
            ActorId = actorId;
        }
    }
}
