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
    /// 攻撃する行動です。
    /// </summary>
    public sealed class AttackAction : RoguelikeAction
    {
        /// <summary>
        /// ねらう相手のID。
        /// </summary>
        public ActorId TargetId { get; }
        /// <summary>
        /// 攻撃の種類（近接・遠距離など）。
        /// </summary>
        public AttackKind Kind { get; }
        /// <summary>
        /// どのくらい遠くまで届くか。
        /// </summary>
        public int Range { get; }

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public AttackAction(ActorId actorId, ActorId targetId, AttackKind kind, int range) : base(actorId)
        {
            TargetId = targetId;
            Kind = kind;
            Range = range;
        }
    }
}
