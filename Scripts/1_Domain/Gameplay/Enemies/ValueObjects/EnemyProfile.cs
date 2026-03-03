// =============================================================================
// EnemyProfile.cs
// =============================================================================
// 概要:
//   敵の定義情報を保持する値オブジェクト。敵の種類（スライム、盗賊等）ごとに
//   このプロファイルを設定します。DDDの値オブジェクトとして不変（readonly struct）
//   で実装されています。
//
// コンテキスト配置:
//   EnemyAIContext/ValueObjects
//   - ActorContextではなくEnemyAIContextに配置している理由:
//     - SightRadius, ForgetTurns, FleeHpThreshold等はAI行動パラメータ
//     - Actorのアイデンティティ（HP、位置等）とは独立した概念
//     - AIシステムの変更時に影響を受けるのはこのコンテキストのみ
//
// プロファイルパラメータ:
//   基本情報:
//     - Id: 内部識別子（例: "slime"）
//     - DisplayName: 表示名（例: "スライム"）
//     - BaseHp/Attack/Defense: 基本ステータス
//
//   視覚・認知:
//     - SightRadius: 視界半径（タイル数）
//     - ForgetTurns: プレイヤーを見失ってから忘れるまでのターン数
//     - WakeDistance: 睡眠状態から起きる距離
//
//   戦闘:
//     - AttackRange: 攻撃射程（1=近接、2以上=遠距離）
//     - PreferredDistance: 好む間合い（遠距離型は距離を保とうとする）
//     - FleeHpThresholdPercent: 逃走を開始するHP割合（0=逃走しない）
//
//   行動パターン:
//     - Speed: 行動速度（Normal, Fast, Slow等）
//     - Intelligence: 知性レベル（パス探索の有無等に影響）
//     - InitialState: 初期AI状態（Sleeping等）
//     - SpecialAbilities: 特殊能力リスト
// =============================================================================

using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Enemies.Enums;

namespace Roguelike.Domain.Gameplay.Enemies.ValueObjects
{
    /// <summary>
    /// 敵の定義情報を保持する値オブジェクトです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 敵の種類ごとにこのプロファイルを設定します。
    /// DDDの値オブジェクトとして不変（readonly struct）で実装されています。
    /// </para>
    /// <para>
    /// ActorContextではなくEnemyAIContextに配置している理由は、
    /// これらのパラメータがAI行動に関する関心事であり、
    /// Actorのアイデンティティとは独立しているためです。
    /// </para>
    /// </remarks>
    public readonly struct EnemyProfile
    {
        /// <summary>
        /// 敵の識別子（例: "slime", "thief"）。
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 表示名（例: "スライム", "盗賊"）。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 行動速度タイプ。
        /// </summary>
        public SpeedType Speed { get; }

        /// <summary>
        /// 視界半径（タイル数）。
        /// </summary>
        public int SightRadius { get; }

        /// <summary>
        /// 攻撃射程（タイル数）。1なら近接のみ。
        /// </summary>
        public int AttackRange { get; }

        /// <summary>
        /// 好む間合い（タイル数）。
        /// この距離を保とうとします。近接型なら1。
        /// </summary>
        public int PreferredDistance { get; }

        /// <summary>
        /// 知性レベル。
        /// </summary>
        public IntelligenceLevel Intelligence { get; }

        /// <summary>
        /// 特殊能力のリスト。
        /// </summary>
        public IReadOnlyList<SpecialAbility> SpecialAbilities { get; }

        /// <summary>
        /// 逃走を開始するHP割合（0-100）。
        /// 0なら逃走しません。
        /// </summary>
        public int FleeHpThresholdPercent { get; }

        /// <summary>
        /// プレイヤーを見失ってから忘れるまでのターン数。
        /// </summary>
        public int ForgetTurns { get; }

        /// <summary>
        /// 睡眠状態から起きる距離（タイル数）。
        /// 0なら最初から起きています。
        /// </summary>
        public int WakeDistance { get; }

        /// <summary>
        /// 初期状態。Sleepingなら最初は寝ています。
        /// </summary>
        public AiState InitialState { get; }

        /// <summary>
        /// 基本HP。
        /// </summary>
        public int BaseHp { get; }

        /// <summary>
        /// 基本攻撃力。
        /// </summary>
        public int BaseAttack { get; }

        /// <summary>
        /// 基本防御力。
        /// </summary>
        public int BaseDefense { get; }

        /// <summary>
        /// EnemyProfileを作成します。
        /// </summary>
        public EnemyProfile(
            string id,
            string displayName,
            SpeedType speed,
            int sightRadius,
            int attackRange,
            int preferredDistance,
            IntelligenceLevel intelligence,
            IReadOnlyList<SpecialAbility> specialAbilities,
            int fleeHpThresholdPercent,
            int forgetTurns,
            int wakeDistance,
            AiState initialState,
            int baseHp,
            int baseAttack,
            int baseDefense)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            if (string.IsNullOrEmpty(displayName))
                throw new ArgumentException("DisplayName cannot be null or empty.", nameof(displayName));
            if (sightRadius < 0)
                throw new ArgumentOutOfRangeException(nameof(sightRadius), "SightRadius must be >= 0.");
            if (attackRange < 1)
                throw new ArgumentOutOfRangeException(nameof(attackRange), "AttackRange must be >= 1.");
            if (preferredDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(preferredDistance), "PreferredDistance must be >= 0.");
            if (fleeHpThresholdPercent < 0 || fleeHpThresholdPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(fleeHpThresholdPercent), "FleeHpThresholdPercent must be 0-100.");
            if (forgetTurns < 0)
                throw new ArgumentOutOfRangeException(nameof(forgetTurns), "ForgetTurns must be >= 0.");
            if (wakeDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(wakeDistance), "WakeDistance must be >= 0.");
            if (baseHp < 1)
                throw new ArgumentOutOfRangeException(nameof(baseHp), "BaseHp must be >= 1.");
            if (baseAttack < 0)
                throw new ArgumentOutOfRangeException(nameof(baseAttack), "BaseAttack must be >= 0.");
            if (baseDefense < 0)
                throw new ArgumentOutOfRangeException(nameof(baseDefense), "BaseDefense must be >= 0.");

            Id = id;
            DisplayName = displayName;
            Speed = speed;
            SightRadius = sightRadius;
            AttackRange = attackRange;
            PreferredDistance = preferredDistance;
            Intelligence = intelligence;
            SpecialAbilities = CopyAbilities(specialAbilities);
            FleeHpThresholdPercent = fleeHpThresholdPercent;
            ForgetTurns = forgetTurns;
            WakeDistance = wakeDistance;
            InitialState = initialState;
            BaseHp = baseHp;
            BaseAttack = baseAttack;
            BaseDefense = baseDefense;
        }

        /// <summary>
        /// 指定した特殊能力を持っているかを判定します。
        /// </summary>
        public bool HasAbility(SpecialAbility ability)
        {
            if (SpecialAbilities == null) return false;
            for (int i = 0; i < SpecialAbilities.Count; i++)
            {
                if (SpecialAbilities[i] == ability)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 近接攻撃タイプかを判定します。
        /// </summary>
        public bool IsMeleeType => AttackRange == 1 && PreferredDistance <= 1;

        /// <summary>
        /// 遠距離攻撃タイプかを判定します。
        /// </summary>
        public bool IsRangedType => AttackRange > 1 || HasAbility(SpecialAbility.RangedAttack);

        private static IReadOnlyList<SpecialAbility> CopyAbilities(IReadOnlyList<SpecialAbility> specialAbilities)
        {
            if (specialAbilities == null || specialAbilities.Count == 0)
            {
                return Array.Empty<SpecialAbility>();
            }

            var copied = new SpecialAbility[specialAbilities.Count];
            for (var i = 0; i < specialAbilities.Count; i++)
            {
                copied[i] = specialAbilities[i];
            }

            return copied;
        }
    }
}


