namespace Roguelike.Application.Enums
{
    /// <summary>
    /// ラン全体のライフサイクルイベント種別です。
    /// </summary>
    public enum RunLifecycleEventKind
    {
        None = 0,
        RunCleared = 1,
        RunGameOver = 2,
        Unknown = 3
    }
}
