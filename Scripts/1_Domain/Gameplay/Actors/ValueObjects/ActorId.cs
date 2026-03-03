using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;

namespace Roguelike.Domain.Gameplay.Actors.ValueObjects
{
    /// <summary>
    /// キャラクターの「名札」のようなIDです。
    /// だれなのかをまちがえないために使います。
    /// </summary>
    public readonly struct ActorId : IEquatable<ActorId>
    {
        /// <summary>
        /// 実体は Guid（ほぼかぶらない長い番号）です。
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// IDを作るときの入口です。
        /// 空の番号は禁止しています。
        /// </summary>
        public ActorId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Actor ID cannot be empty.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// 新しいIDを作ります。
        /// </summary>
        public static ActorId NewId() => new ActorId(Guid.NewGuid());

        /// <summary>
        /// ID同士が同じかを比べます。
        /// </summary>
        public bool Equals(ActorId other) => Value.Equals(other.Value);

        /// <summary>
        /// objectとして渡されたときの比較です。
        /// </summary>
        public override bool Equals(object obj) => obj is ActorId other && Equals(other);

        /// <summary>
        /// ハッシュ用の数字を返します。
        /// </summary>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// 文字にして見やすくします。
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// == で比べられるようにします。
        /// </summary>
        public static bool operator ==(ActorId left, ActorId right) => left.Equals(right);

        /// <summary>
        /// != で比べられるようにします。
        /// </summary>
        public static bool operator !=(ActorId left, ActorId right) => !left.Equals(right);
    }
}


