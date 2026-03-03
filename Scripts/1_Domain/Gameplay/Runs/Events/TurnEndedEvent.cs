namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// ターンが終わったことを知らせる出来事です。
    /// </summary>
    public sealed class TurnEndedEvent : IRoguelikeEvent
    {
        /// <summary>
        /// 終わったターンの番号。
        /// </summary>
        public int TurnNumber { get; }

        /// <summary>
        /// 出来事を作るときの入口です。
        /// </summary>
        public TurnEndedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
}


