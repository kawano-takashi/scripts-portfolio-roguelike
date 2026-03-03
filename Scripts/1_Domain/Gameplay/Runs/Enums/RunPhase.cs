using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;
namespace Roguelike.Domain.Gameplay.Runs.Enums
{
    /// <summary>
    /// いまゲームがどの状態かを表します。
    /// </summary>
    public enum RunPhase
    {
        /// <summary>
        /// 走り出し（最初の準備）。
        /// </summary>
        RunStart,
        /// <summary>
        /// 探索中（ふつうのプレイ中）。
        /// UIの開閉はPresentation側のRunUiControllerで管理します。
        /// </summary>
        InRun,
        /// <summary>
        /// 一時停止。
        /// </summary>
        Pause,
        /// <summary>
        /// ゲームオーバー（負け）。
        /// </summary>
        GameOver,
        /// <summary>
        /// クリア（勝ち）。
        /// </summary>
        Clear
    }
}
