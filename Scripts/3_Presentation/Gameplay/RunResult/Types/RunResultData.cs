namespace Roguelike.Presentation.Gameplay.RunResult.Types
{
    /// <summary>
    /// 結果画面に表示するデータをまとめたクラスです。
    /// クリア時・ゲームオーバー時にResultStoreから生成され、
    /// RunResultViewが表示に使用します。
    /// </summary>
    public sealed class RunResultData
    {
        /// <summary>
        /// クリアしたかどうか（trueならクリア、falseならゲームオーバー）。
        /// </summary>
        public bool IsVictory { get; }

        /// <summary>
        /// 到達した階層（クリア時はクリア階、ゲームオーバー時は死亡階）。
        /// </summary>
        public int FinalFloor { get; }

        /// <summary>
        /// 冒険にかかった総ターン数。
        /// </summary>
        public int TotalTurns { get; }

        /// <summary>
        /// 終了時のプレイヤーレベル。
        /// </summary>
        public int PlayerLevel { get; }

        /// <summary>
        /// 結果データを作ります。
        /// </summary>
        public RunResultData(bool isVictory, int finalFloor, int totalTurns, int playerLevel)
        {
            IsVictory = isVictory;
            FinalFloor = finalFloor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
        }
    }
}



