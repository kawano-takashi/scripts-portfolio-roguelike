namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// ダンジョンをクリアしたときに発生するラン終了イベントです。
    /// </summary>
    public sealed class RunClearedEvent : IRunLifecycleEvent
    {
        /// <summary>
        /// クリア時の階層。
        /// </summary>
        public int FinalFloor { get; }

        /// <summary>
        /// クリアまでにかかった総ターン数。
        /// </summary>
        public int TotalTurns { get; }

        /// <summary>
        /// クリア時のプレイヤーレベル。
        /// </summary>
        public int PlayerLevel { get; }

        /// <summary>
        /// クリアイベントを作ります。
        /// </summary>
        public RunClearedEvent(int finalFloor, int totalTurns, int playerLevel)
        {
            FinalFloor = finalFloor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
        }
    }
}


