namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// プレイヤーが死亡してゲームオーバーになったときに発生するラン終了イベントです。
    /// </summary>
    public sealed class RunGameOverEvent : IRunLifecycleEvent
    {
        /// <summary>
        /// 死亡した階層。
        /// </summary>
        public int Floor { get; }

        /// <summary>
        /// 死亡までにかかった総ターン数。
        /// </summary>
        public int TotalTurns { get; }

        /// <summary>
        /// 死亡時のプレイヤーレベル。
        /// </summary>
        public int PlayerLevel { get; }

        /// <summary>
        /// ゲームオーバーイベントを作ります。
        /// </summary>
        public RunGameOverEvent(int floor, int totalTurns, int playerLevel)
        {
            Floor = floor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
        }
    }
}


