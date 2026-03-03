namespace Roguelike.Presentation.Gameplay.RunResult.DisplayModels
{
    /// <summary>
    /// ラン結果画面向けの表示モデルです。
    /// </summary>
    public sealed class RunResultDisplayModel
    {
        public bool IsVictory { get; }
        public int FinalFloor { get; }
        public int TotalTurns { get; }
        public int PlayerLevel { get; }

        public RunResultDisplayModel(bool isVictory, int finalFloor, int totalTurns, int playerLevel)
        {
            IsVictory = isVictory;
            FinalFloor = finalFloor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
        }
    }
}



