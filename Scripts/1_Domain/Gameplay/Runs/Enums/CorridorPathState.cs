namespace Roguelike.Domain.Gameplay.Runs.Enums
{
    /// <summary>
    /// 通路上で前進先を探索した結果です。
    /// </summary>
    public enum CorridorPathState
    {
        /// <summary>
        /// 前進候補が1方向のみ。
        /// </summary>
        SinglePath = 0,
        /// <summary>
        /// 前進候補が2方向以上あり分岐している。
        /// </summary>
        Junction,
        /// <summary>
        /// 前進候補が存在しない。
        /// </summary>
        DeadEnd
    }
}


