using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;

namespace Roguelike.Domain.Gameplay.Actors.ValueObjects
{
    /// <summary>
    /// キャラクターの「基本能力」をまとめた箱です。
    /// </summary>
    public readonly struct ActorStats : IEquatable<ActorStats>
    {
        /// <summary>
        /// 最大HP（体力の上限）。
        /// </summary>
        public int MaxHp { get; }
        /// <summary>
        /// 攻撃力。
        /// </summary>
        public int Attack { get; }
        /// <summary>
        /// 防御力。
        /// </summary>
        public int Defense { get; }
        /// <summary>
        /// 知力。空腹度コストや成功率に関わる想定。
        /// </summary>
        public int Intelligence { get; }
        /// <summary>
        /// 見える範囲（何マス先まで見えるか）。
        /// </summary>
        public int SightRadius { get; }
        /// <summary>
        /// 空腹度の最大値。
        /// </summary>
        public float MaxHunger { get; }

        /// <summary>
        /// 能力値をまとめて作るときの入口です。
        /// 変な値（マイナスなど）は禁止しています。
        /// </summary>
        public ActorStats(int maxHp, int attack, int defense, int intelligence, int sightRadius, float maxHunger)
        {
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp), "MaxHp must be positive.");
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack), "Attack must be non-negative.");
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense), "Defense must be non-negative.");
            if (intelligence < 0) throw new ArgumentOutOfRangeException(nameof(intelligence), "Intelligence must be non-negative.");
            if (sightRadius <= 0) throw new ArgumentOutOfRangeException(nameof(sightRadius), "SightRadius must be positive.");
            if (maxHunger < 0) throw new ArgumentOutOfRangeException(nameof(maxHunger), "MaxHunger must be non-negative.");

            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            Intelligence = intelligence;
            SightRadius = sightRadius;
            MaxHunger = maxHunger;
        }

        /// <summary>
        /// 同じ能力かどうかを比べます。
        /// </summary>
        public bool Equals(ActorStats other)
        {
            return MaxHp == other.MaxHp &&
                   Attack == other.Attack &&
                   Defense == other.Defense &&
                   Intelligence == other.Intelligence &&
                   SightRadius == other.SightRadius &&
                   MaxHunger == other.MaxHunger;
        }

        /// <summary>
        /// objectとして比べるときの処理です。
        /// </summary>
        public override bool Equals(object obj) => obj is ActorStats other && Equals(other);

        /// <summary>
        /// ハッシュ用の数字をまとめて作ります。
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(MaxHp, Attack, Defense, Intelligence, SightRadius, MaxHunger);
        }

        /// <summary>
        /// == で比べられるようにします。
        /// </summary>
        public static bool operator ==(ActorStats left, ActorStats right) => left.Equals(right);

        /// <summary>
        /// != で比べられるようにします。
        /// </summary>
        public static bool operator !=(ActorStats left, ActorStats right) => !left.Equals(right);

        /// <summary>
        /// 文字にして見やすくします。
        /// </summary>
        public override string ToString()
        {
            return $"Stats(HP:{MaxHp}, ATK:{Attack}, DEF:{Defense}, INT:{Intelligence}, SIGHT:{SightRadius}, HUNGER:{MaxHunger})";
        }
    }
}


