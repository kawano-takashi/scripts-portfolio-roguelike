using System;

namespace Roguelike.Domain.Gameplay.Actors.ValueObjects
{
    /// <summary>
    /// レベルアップ時のステータス成長率を表す値オブジェクトです。
    /// </summary>
    public readonly struct StatGrowth : IEquatable<StatGrowth>
    {
        /// <summary>
        /// レベルあたりの最大HP上昇量。
        /// </summary>
        public int MaxHpGrowth { get; }

        /// <summary>
        /// レベルあたりの攻撃力上昇量。
        /// </summary>
        public int AttackGrowth { get; }

        /// <summary>
        /// レベルあたりの防御力上昇量。
        /// </summary>
        public int DefenseGrowth { get; }

        /// <summary>
        /// プレイヤーのデフォルト成長率（HP+3, ATK+1, DEF+1）。
        /// </summary>
        public static StatGrowth PlayerDefault => new(3, 1, 1);

        /// <summary>
        /// 成長なし（敵など）。
        /// </summary>
        public static StatGrowth None => new(0, 0, 0);

        /// <summary>
        /// 成長率を作ります。
        /// </summary>
        public StatGrowth(int maxHpGrowth, int attackGrowth, int defenseGrowth)
        {
            if (maxHpGrowth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHpGrowth), "MaxHpGrowth must be non-negative.");
            }

            if (attackGrowth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackGrowth), "AttackGrowth must be non-negative.");
            }

            if (defenseGrowth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defenseGrowth), "DefenseGrowth must be non-negative.");
            }

            MaxHpGrowth = maxHpGrowth;
            AttackGrowth = attackGrowth;
            DefenseGrowth = defenseGrowth;
        }

        /// <summary>
        /// 指定レベルでの累計ステータス上昇量を計算します。
        /// レベル1から数えるので、レベル2で初めて成長分が加算されます。
        /// </summary>
        public (int hpBonus, int attackBonus, int defenseBonus) CalculateBonusForLevel(int level)
        {
            if (level <= 1)
            {
                return (0, 0, 0);
            }

            var levelsGained = level - 1;
            return (
                MaxHpGrowth * levelsGained,
                AttackGrowth * levelsGained,
                DefenseGrowth * levelsGained
            );
        }

        public bool Equals(StatGrowth other)
        {
            return MaxHpGrowth == other.MaxHpGrowth &&
                   AttackGrowth == other.AttackGrowth &&
                   DefenseGrowth == other.DefenseGrowth;
        }

        public override bool Equals(object obj) => obj is StatGrowth other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(MaxHpGrowth, AttackGrowth, DefenseGrowth);

        public static bool operator ==(StatGrowth left, StatGrowth right) => left.Equals(right);

        public static bool operator !=(StatGrowth left, StatGrowth right) => !left.Equals(right);

        public override string ToString()
        {
            return $"StatGrowth(HP+{MaxHpGrowth}, ATK+{AttackGrowth}, DEF+{DefenseGrowth})";
        }
    }
}


