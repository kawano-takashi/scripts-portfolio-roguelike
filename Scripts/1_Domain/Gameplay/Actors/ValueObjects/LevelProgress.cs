using System;

namespace Roguelike.Domain.Gameplay.Actors.ValueObjects
{
    /// <summary>
    /// レベルと経験値の状態を表す値オブジェクトです。
    /// </summary>
    public readonly struct LevelProgress : IEquatable<LevelProgress>
    {
        /// <summary>
        /// 最大レベル。
        /// </summary>
        public const int MaxLevel = 99;

        /// <summary>
        /// 現在のレベル（1〜99）。
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// 現在の経験値。
        /// </summary>
        public int CurrentExp { get; }

        /// <summary>
        /// 次のレベルまでに必要な経験値。
        /// </summary>
        public int ExpToNextLevel { get; }

        /// <summary>
        /// 初期状態（レベル1、経験値0）を返します。
        /// </summary>
        public static LevelProgress Initial => new(1, 0, CalculateExpRequired(1));

        /// <summary>
        /// レベル進行状態を作ります。
        /// </summary>
        public LevelProgress(int level, int currentExp, int expToNextLevel)
        {
            if (level < 1 || level > MaxLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), $"Level must be between 1 and {MaxLevel}.");
            }

            if (currentExp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentExp), "CurrentExp must be non-negative.");
            }

            if (expToNextLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expToNextLevel), "ExpToNextLevel must be non-negative.");
            }

            Level = level;
            CurrentExp = currentExp;
            ExpToNextLevel = expToNextLevel;
        }

        /// <summary>
        /// 指定レベルに到達するために必要な経験値を計算します。
        /// 公式: 20 * level^1.5（風来のシレン風）
        /// </summary>
        public static int CalculateExpRequired(int level)
        {
            if (level < 1 || level >= MaxLevel)
            {
                return 0;
            }

            return (int)(20 * Math.Pow(level, 1.5));
        }

        /// <summary>
        /// 経験値を加算し、レベルアップを処理した新しい状態を返します。
        /// </summary>
        public LevelProgress AddExperience(int amount, out bool leveledUp, out int levelsGained)
        {
            leveledUp = false;
            levelsGained = 0;

            if (amount <= 0 || Level >= MaxLevel)
            {
                return this;
            }

            var newExp = CurrentExp + amount;
            var newLevel = Level;
            var newExpToNext = ExpToNextLevel;

            // レベルアップ判定（連続レベルアップ対応）
            while (newLevel < MaxLevel && newExp >= newExpToNext)
            {
                newExp -= newExpToNext;
                newLevel++;
                levelsGained++;
                newExpToNext = CalculateExpRequired(newLevel);
            }

            if (levelsGained > 0)
            {
                leveledUp = true;
            }

            // 最大レベルに達したら経験値を0にする
            if (newLevel >= MaxLevel)
            {
                newExp = 0;
                newExpToNext = 0;
            }

            return new LevelProgress(newLevel, newExp, newExpToNext);
        }

        public bool Equals(LevelProgress other)
        {
            return Level == other.Level &&
                   CurrentExp == other.CurrentExp &&
                   ExpToNextLevel == other.ExpToNextLevel;
        }

        public override bool Equals(object obj) => obj is LevelProgress other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Level, CurrentExp, ExpToNextLevel);

        public static bool operator ==(LevelProgress left, LevelProgress right) => left.Equals(right);

        public static bool operator !=(LevelProgress left, LevelProgress right) => !left.Equals(right);

        public override string ToString()
        {
            return $"LevelProgress(Lv:{Level}, Exp:{CurrentExp}/{ExpToNextLevel})";
        }
    }
}


