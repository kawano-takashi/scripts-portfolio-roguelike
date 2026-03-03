namespace Roguelike.Application.Enums
{
    /// <summary>
    /// ダッシュ停止理由です。
    /// </summary>
    public enum DashStopReason
    {
        /// <summary>
        /// 停止理由なし（継続可能）。
        /// </summary>
        None = 0,
        /// <summary>
        /// 実行に必要な参照が欠けている無効状態。
        /// </summary>
        InvalidState,
        /// <summary>
        /// 壁・マップ外・角抜け禁止などで前進不可。
        /// </summary>
        BlockedAhead,
        /// <summary>
        /// 同勢力のアクターが前方を占有。
        /// </summary>
        OccupiedAhead,
        /// <summary>
        /// 敵対アクターが前方を占有。
        /// </summary>
        EnemyAhead,
        /// <summary>
        /// 足元にアイテムがある。
        /// </summary>
        OnItem,
        /// <summary>
        /// 足元が下り階段。
        /// </summary>
        OnStairs,
        /// <summary>
        /// 現在いる部屋内に敵対アクターが存在。
        /// </summary>
        EnemySighted,
        /// <summary>
        /// 通路でチェビシェフ距離2以内に敵対アクターが存在。
        /// </summary>
        EnemyNearby,
        /// <summary>
        /// 部屋境界（部屋&lt;-&gt;通路、または部屋間）を跨いだ。
        /// </summary>
        RoomBoundary,
        /// <summary>
        /// 通路の進行候補が複数あり分岐している。
        /// </summary>
        Junction,
        /// <summary>
        /// 通路の進行候補がなく行き止まり。
        /// </summary>
        DeadEnd,
        /// <summary>
        /// 移動実行後に位置変化を確認できず、行動失敗とみなした。
        /// </summary>
        ActionFailed
    }
}
