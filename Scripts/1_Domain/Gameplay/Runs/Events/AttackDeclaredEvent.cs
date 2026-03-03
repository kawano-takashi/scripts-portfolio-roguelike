using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// 攻撃を宣言した出来事です。
    /// 攻撃の成否にかかわらず発行されます。
    /// </summary>
    public sealed class AttackDeclaredEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 攻撃したアクターID。
        /// </summary>
        public ActorId AttackerId { get; }
        /// <summary>
        /// 攻撃対象のアクターID。存在しない場合は null。
        /// </summary>
        public ActorId? TargetId { get; }
        /// <summary>
        /// 攻撃の種類。
        /// </summary>
        public AttackKind Kind { get; }
        /// <summary>
        /// 攻撃側の位置。
        /// </summary>
        public Position AttackerPosition { get; }
        /// <summary>
        /// 攻撃側の向き。
        /// </summary>
        public Direction AttackerFacing { get; }
        /// <summary>
        /// 狙い位置（ターゲットがいない場合は射程末端）。
        /// </summary>
        public Position TargetPosition { get; }
        /// <summary>
        /// 攻撃の射程。
        /// </summary>
        public int Range { get; }

        /// <summary>
        /// 攻撃宣言イベントを作成します。
        /// </summary>
        public AttackDeclaredEvent(
            ActorId attackerId,
            ActorId? targetId,
            AttackKind kind,
            Position attackerPosition,
            Direction attackerFacing,
            Position targetPosition,
            int range)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Kind = kind;
            AttackerPosition = attackerPosition;
            AttackerFacing = attackerFacing;
            TargetPosition = targetPosition;
            Range = range;
        }
    }
}


