namespace Roguelike.Application.Enums
{
    /// <summary>
    /// Application層で公開するラン状態です。
    /// </summary>
    public enum RunPhaseDto
    {
        None = 0,
        RunStart = 1,
        InRun = 2,
        Pause = 3,
        Clear = 4,
        GameOver = 5
    }
}
