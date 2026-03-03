using System;
using UnityEngine;
using VContainer;
using Roguelike.Application.Commands;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Hud.Stores;

namespace Roguelike.Presentation.Gameplay.Shell.Core
{
    /// <summary>
    /// Roguelikeランの初期化を担当するビュー。
    /// Inspectorで設定した値を使い、ランを作成・再利用する。
    /// </summary>
    public class RoguelikeRunInitializer : MonoBehaviour
    {
        [Header("Roguelike Run")]
        /// <summary>Roguelikeモードを使用するかどうか</summary>
        [SerializeField] private bool _useRoguelikeRun = true;

        /// <summary>マップ生成のシード値（0の場合はランダム）</summary>
        [SerializeField] private int _roguelikeSeed = 0;

        /// <summary>マップの横幅</summary>
        [SerializeField] private int _roguelikeWidth = 48;

        /// <summary>マップの縦幅</summary>
        [SerializeField] private int _roguelikeHeight = 48;

        /// <summary>クリア階層</summary>
        [SerializeField] private int _clearFloor = 10;

        /// <summary>Roguelikeラン開始を実行するユースケース</summary>
        [Inject] private StartRunCommandHandler _startRunCommandHandler;
        /// <summary>Roguelikeターン表示状態ストア</summary>
        [Inject] private RunTurnStateStore _runTurnStateStore;
        /// <summary>結果状態ストア</summary>
        [Inject] private RunResultStore _runResultStore;
        /// <summary>ラン状態スナップショット参照ポート</summary>
        [Inject] private RunSnapshotQueryHandler _runSnapshotQueryHandler;
        /// <summary>ラン状態ストア</summary>
        [Inject] private RunStatusStore _runStatusStore;
        /// <summary>ランログストア</summary>
        [Inject] private RunLogStore _runLogStore;

        /// <summary>
        /// 外部から呼び出される初期化メソッド。
        /// Roguelikeモードが有効な場合、ランの初期化を行う。
        /// </summary>
        public void Init()
        {
            if (!UseRoguelikeRun())
            {
                return;
            }

            InitializeRoguelikeRun();
        }

        /// <summary>
        /// デバッグ用のダンジョン生成エントリポイント。
        /// Inspectorのボタンなどから手動で呼び出して使用する。
        /// </summary>
        /// <param name="width">マップの横幅</param>
        /// <param name="height">マップの縦幅</param>
        /// <param name="seed">乱数シード（nullの場合はランダム）</param>
        /// <param name="mapId">マップID（現在未使用）</param>
        public void GenerateAndRenderDungeon(int width, int height, int? seed = null, int mapId = 1)
        {
            try
            {
                if (!UseRoguelikeRun())
                {
                    return;
                }

                GenerateRoguelikeRun(width, height, seed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ダンジョン生成中にエラーが発生: {ex.Message}");
            }
        }

        /// <summary>
        /// Roguelikeランを初期化する。
        /// 既存のランがあれば再利用し、なければ新規作成する。
        /// </summary>
        private void InitializeRoguelikeRun()
        {
            if (_runSnapshotQueryHandler != null)
            {
                var currentSnapshotResult = _runSnapshotQueryHandler.Handle();
                if (currentSnapshotResult.IsSuccess && currentSnapshotResult.Value.HasRun)
                {
                    _runTurnStateStore?.InitializeFromSnapshot(currentSnapshotResult.Value);
                    _runStatusStore?.Refresh();
                    _runLogStore?.RefreshActorNameCacheFromReadModel();
                    return;
                }
            }

            var seed = _roguelikeSeed == 0 ? (int?)null : _roguelikeSeed;
            var width = _roguelikeWidth > 0 ? (int?)_roguelikeWidth : null;
            var height = _roguelikeHeight > 0 ? (int?)_roguelikeHeight : null;

            var command = CreateDefaultStartRunCommand(seed, width, height);
            var startResult = _startRunCommandHandler.Handle(command);
            if (!startResult.IsSuccess)
            {
                Debug.LogWarning($"StartRun failed: {startResult.ErrorMessage}");
                return;
            }

            _runResultStore?.ApplyLifecycleEvents(startResult.Value.LifecycleEvents);

            // ターン状態と表示ストアを最新ランへ同期。
            var snapshotResult = _runSnapshotQueryHandler.Handle();
            if (snapshotResult.IsSuccess)
            {
                _runTurnStateStore?.InitializeFromSnapshot(snapshotResult.Value);
            }
            _runStatusStore?.Refresh();
            _runLogStore?.ClearHistory();
            _runLogStore?.RefreshActorNameCacheFromReadModel();
        }

        /// <summary>
        /// Roguelikeランを生成する（デバッグ用）。
        /// </summary>
        private void GenerateRoguelikeRun(int width, int height, int? seed)
        {
            var widthOrNull = width > 0 ? (int?)width : null;
            var heightOrNull = height > 0 ? (int?)height : null;
            var command = CreateDefaultStartRunCommand(seed, widthOrNull, heightOrNull);
            var startResult = _startRunCommandHandler.Handle(command);
            if (!startResult.IsSuccess)
            {
                Debug.LogWarning($"StartRun failed: {startResult.ErrorMessage}");
                return;
            }

            _runResultStore?.ApplyLifecycleEvents(startResult.Value.LifecycleEvents);

            // ターン状態と表示ストアを最新ランへ同期。
            var snapshotResult = _runSnapshotQueryHandler.Handle();
            if (snapshotResult.IsSuccess)
            {
                _runTurnStateStore?.InitializeFromSnapshot(snapshotResult.Value);
            }
            _runStatusStore?.Refresh();
            _runLogStore?.ClearHistory();
            _runLogStore?.RefreshActorNameCacheFromReadModel();
        }

        /// <summary>
        /// デバッグ用のデフォルトラン開始コマンドを作成する。
        /// </summary>
        private StartRunCommand CreateDefaultStartRunCommand(int? seed, int? width, int? height)
        {
            return new StartRunCommand(
                PlayerName: "魔術師",
                Floor: 1,
                ClearFloor: _clearFloor,
                Seed: seed,
                Width: width,
                Height: height,
                StartImmediately: true,
                PlayerMaxHp: 20,
                PlayerAttack: 3,
                PlayerDefense: 1,
                PlayerIntelligence: 14,
                PlayerSightRadius: 8,
                PlayerMaxHunger: 100f);
        }

        /// <summary>
        /// Roguelike表示モードを使用するかどうかを判定する。
        /// </summary>
        private bool UseRoguelikeRun()
        {
            return _useRoguelikeRun && _startRunCommandHandler != null;
        }
    }
}





