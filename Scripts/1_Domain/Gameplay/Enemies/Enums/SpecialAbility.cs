// =============================================================================
// SpecialAbility.cs
// =============================================================================
// 概要:
//   敵の特殊能力を表す列挙型。敵プロファイルで複数指定可能です。
//   トルネコ/シレン風の多彩な敵行動を実現するための能力定義。
//
// 能力カテゴリ:
//   移動系:
//     - WallFollow: 壁沿いに移動（地蜘蛛タイプ）
//     - Teleport: ワープ移動（ワープ系モンスター）
//     - PassWall: 壁をすり抜け（ゴースト）
//     - WaterWalk: 水上移動（水棲系）
//
//   攻撃系:
//     - RangedAttack: 遠距離攻撃（弓兵、魔法使い）
//     - MultiAttack: 複数回攻撃（双頭系）
//     - ThrowItem: アイテム投擲（投げ系）
//
//   状態異常系:
//     - InflictConfuse: 混乱付与
//     - InflictPoison: 毒付与
//     - InflictSleep: 睡眠付与
//     - InflictSilence: 沈黙付与
//     - DrainHp: HP吸収
//
//   特殊行動系:
//     - StealItem: アイテム盗み（盗賊）
//     - Divide: 分裂（スライム）
//     - CallAlly: 仲間呼び（親分系）
//     - Explode: 自爆（ばくだん岩）
//     - Ambush: 待ち伏せ（隠れ系）
//     - Counter: 反撃
//     - Resurrect: 復活
//     - Transform: 変身
//
// 実装での使用:
//   - EnemyProfile.SpecialAbilities: 敵種別ごとの能力リスト
//   - EnemyProfile.HasAbility(): 特定能力を持つかの判定
//   - 各Behavior実装: 能力に応じた行動選択
// =============================================================================

namespace Roguelike.Domain.Gameplay.Enemies.Enums
{
    /// <summary>
    /// 敵の特殊能力を表します。
    /// </summary>
    /// <remarks>
    /// 敵プロファイルで複数指定可能です。
    /// トルネコ/シレン風の多彩な敵行動を実現するための能力定義です。
    /// </remarks>
    public enum SpecialAbility
    {
        /// <summary>
        /// 特殊能力なし。
        /// </summary>
        None,

        // === 移動系 ===

        /// <summary>
        /// 壁沿いに移動する。
        /// </summary>
        WallFollow,

        /// <summary>
        /// ワープ移動できる。
        /// </summary>
        Teleport,

        /// <summary>
        /// 壁をすり抜けられる（ゴースト）。
        /// </summary>
        PassWall,

        /// <summary>
        /// 水上を移動できる。
        /// </summary>
        WaterWalk,

        // === 攻撃系 ===

        /// <summary>
        /// 遠距離攻撃ができる。
        /// </summary>
        RangedAttack,

        /// <summary>
        /// 1ターンに複数回攻撃する。
        /// </summary>
        MultiAttack,

        /// <summary>
        /// アイテムを投げてくる。
        /// </summary>
        ThrowItem,

        // === 状態異常系 ===

        /// <summary>
        /// 混乱を付与する攻撃。
        /// </summary>
        InflictConfuse,

        /// <summary>
        /// 毒を付与する攻撃。
        /// </summary>
        InflictPoison,

        /// <summary>
        /// 睡眠を付与する攻撃。
        /// </summary>
        InflictSleep,

        /// <summary>
        /// 沈黙を付与する攻撃。
        /// </summary>
        InflictSilence,

        /// <summary>
        /// HP吸収攻撃。
        /// </summary>
        DrainHp,

        // === 特殊行動系 ===

        /// <summary>
        /// アイテムを盗む（盗賊）。
        /// </summary>
        StealItem,

        /// <summary>
        /// 分裂する（スライム）。
        /// </summary>
        Divide,

        /// <summary>
        /// 仲間を呼ぶ（親分）。
        /// </summary>
        CallAlly,

        /// <summary>
        /// 自爆する（ばくだん岩）。
        /// </summary>
        Explode,

        /// <summary>
        /// 待ち伏せする（隠れている）。
        /// </summary>
        Ambush,

        /// <summary>
        /// 攻撃を受けると反撃する。
        /// </summary>
        Counter,

        /// <summary>
        /// 一度倒されても復活する。
        /// </summary>
        Resurrect,

        /// <summary>
        /// 別の敵に変身する。
        /// </summary>
        Transform
    }
}


