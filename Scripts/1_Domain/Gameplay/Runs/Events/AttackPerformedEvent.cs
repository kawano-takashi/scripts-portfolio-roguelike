using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 攻撃が成立した出来事です。
    /// 演出やUIが「命中した」ことを検知するために使います。
    /// </summary>
    public sealed class AttackPerformedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 攻撃したアクターID。
        /// </summary>
        public ActorId AttackerId { get; }
        /// <summary>
        /// 攻撃されたアクターID。
        /// </summary>
        public ActorId TargetId { get; }
        /// <summary>
        /// 攻撃の種類。
        /// </summary>
        public AttackKind Kind { get; }
        /// <summary>
        /// 攻撃側の位置。
        /// </summary>
        public Position AttackerPosition { get; }
        /// <summary>
        /// 防御側の位置。
        /// </summary>
        public Position TargetPosition { get; }
        /// <summary>
        /// 攻撃の発生源。
        /// </summary>
        public AttackSource Source { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public AttackPerformedEvent(
            ActorId attackerId,
            ActorId targetId,
            AttackKind kind,
            Position attackerPosition,
            Position targetPosition,
            AttackSource source)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Kind = kind;
            AttackerPosition = attackerPosition;
            TargetPosition = targetPosition;
            Source = source;
        }
    }
}


