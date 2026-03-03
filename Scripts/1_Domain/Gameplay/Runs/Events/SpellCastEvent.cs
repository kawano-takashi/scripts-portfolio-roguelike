using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// スペル詠唱を宣言した出来事です。
    /// 演出やUIが「スペルが発動した」ことを検知するために使います。
    /// </summary>
    public sealed class SpellCastEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 詠唱したアクターID。
        /// </summary>
        public ActorId CasterId { get; }
        /// <summary>
        /// 詠唱したスペルの種類。
        /// </summary>
        public ItemId Spell { get; }
        /// <summary>
        /// 詠唱者の位置。
        /// </summary>
        public Position CasterPosition { get; }
        /// <summary>
        /// 詠唱者の向いている方向。
        /// </summary>
        public Direction CasterFacing { get; }
        /// <summary>
        /// ターゲットの位置。ターゲットがいない場合は射程末端。
        /// </summary>
        public Position TargetPosition { get; }
        /// <summary>
        /// ターゲットのアクターID。ターゲットがいない場合はnull。
        /// </summary>
        public ActorId? TargetId { get; }
        /// <summary>
        /// スペルの射程。
        /// </summary>
        public int Range { get; }
        /// <summary>
        /// 装備中の魔法書から詠唱したかどうか。
        /// true: 装備発動（接触時の自動詠唱など）
        /// false: 非装備発動（インベントリ使用や通常詠唱アクション）
        /// </summary>
        public bool IsEquippedSpellCast { get; }

        /// <summary>
        /// スペル詠唱イベントを作成します。
        /// </summary>
        public SpellCastEvent(
            ActorId casterId,
            ItemId spell,
            Position casterPosition,
            Direction casterFacing,
            Position targetPosition,
            ActorId? targetId,
            int range,
            bool isEquippedSpellCast)
        {
            CasterId = casterId;
            Spell = spell;
            CasterPosition = casterPosition;
            CasterFacing = casterFacing;
            TargetPosition = targetPosition;
            TargetId = targetId;
            Range = range;
            // 演出側が「詠唱前ウェイトを入れるか」を判断できるように保持します。
            IsEquippedSpellCast = isEquippedSpellCast;
        }
    }
}


