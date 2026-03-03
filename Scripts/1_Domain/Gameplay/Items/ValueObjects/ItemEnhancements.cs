using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Roguelike.Domain.Gameplay.Items.Enums;

namespace Roguelike.Domain.Gameplay.Items.ValueObjects
{
    /// <summary>
    /// アイテム個体が持つ強化レベルの集合です（不変オブジェクト）。
    /// 複数の強化軸を同時に保持できます。
    /// </summary>
    public sealed class ItemEnhancements : IEquatable<ItemEnhancements>
    {
        /// <summary>
        /// 強化レベルの上限。
        /// </summary>
        public const int MaxLevel = 10;

        /// <summary>
        /// 強化なしのインスタンス。
        /// </summary>
        public static readonly ItemEnhancements None = new(new Dictionary<SpellEnhancementCategory, int>());

        private readonly Dictionary<SpellEnhancementCategory, int> _levels;
        private readonly IReadOnlyDictionary<SpellEnhancementCategory, int> _readOnlyLevels;

        /// <summary>
        /// 各カテゴリの強化レベル（読み取り専用）。
        /// </summary>
        public IReadOnlyDictionary<SpellEnhancementCategory, int> Levels => _readOnlyLevels;

        /// <summary>
        /// 強化が1つでもあるかどうか。
        /// </summary>
        public bool HasAny => _levels.Count > 0;

        private ItemEnhancements(Dictionary<SpellEnhancementCategory, int> levels)
        {
            _levels = levels == null
                ? new Dictionary<SpellEnhancementCategory, int>()
                : new Dictionary<SpellEnhancementCategory, int>(levels);
            _readOnlyLevels = new ReadOnlyDictionary<SpellEnhancementCategory, int>(_levels);
        }

        /// <summary>
        /// 指定カテゴリの強化レベルを返します。未強化なら0。
        /// </summary>
        public int GetLevel(SpellEnhancementCategory category)
        {
            return _levels.TryGetValue(category, out var level) ? level : 0;
        }

        /// <summary>
        /// 指定カテゴリを1レベル強化した新しいインスタンスを返します。
        /// 上限に達していたら元のインスタンスをそのまま返します。
        /// </summary>
        public ItemEnhancements Enhance(SpellEnhancementCategory category)
        {
            var currentLevel = GetLevel(category);
            if (currentLevel >= MaxLevel)
            {
                return this;
            }

            var newLevels = new Dictionary<SpellEnhancementCategory, int>(_levels);
            newLevels[category] = currentLevel + 1;
            return new ItemEnhancements(newLevels);
        }

        /// <summary>
        /// 指定カテゴリを指定レベルに設定した新しいインスタンスを返します。
        /// </summary>
        public ItemEnhancements WithLevel(SpellEnhancementCategory category, int level)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            var clampedLevel = Math.Min(level, MaxLevel);
            var newLevels = new Dictionary<SpellEnhancementCategory, int>(_levels);

            if (clampedLevel == 0)
            {
                newLevels.Remove(category);
            }
            else
            {
                newLevels[category] = clampedLevel;
            }

            return new ItemEnhancements(newLevels);
        }

        /// <summary>
        /// 複数カテゴリをまとめて設定します。
        /// </summary>
        public static ItemEnhancements Create(params (SpellEnhancementCategory category, int level)[] entries)
        {
            var levels = new Dictionary<SpellEnhancementCategory, int>();
            foreach (var (category, level) in entries)
            {
                if (level > 0)
                {
                    levels[category] = Math.Min(level, MaxLevel);
                }
            }

            return levels.Count == 0 ? None : new ItemEnhancements(levels);
        }

        public bool Equals(ItemEnhancements other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (_levels.Count != other._levels.Count)
            {
                return false;
            }

            foreach (var kvp in _levels)
            {
                if (!other._levels.TryGetValue(kvp.Key, out var otherLevel) || kvp.Value != otherLevel)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemEnhancements other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            foreach (var kvp in _levels.OrderBy(k => k.Key))
            {
                hash = HashCode.Combine(hash, kvp.Key, kvp.Value);
            }

            return hash;
        }
    }
}
