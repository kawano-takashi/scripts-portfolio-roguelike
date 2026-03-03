namespace Roguelike.Domain.Gameplay.Runs.Enums
{
    /// <summary>
    /// ダッシュ停止理由（ドメイン側）です。
    /// </summary>
    public enum DashContinuationStopReason
    {
        None = 0,
        InvalidState,
        BlockedAhead,
        OccupiedAhead,
        EnemyAhead,
        OnItem,
        OnStairs,
        EnemySighted,
        EnemyNearby,
        RoomBoundary,
        Junction,
        DeadEnd,
        ActionFailed
    }
}


