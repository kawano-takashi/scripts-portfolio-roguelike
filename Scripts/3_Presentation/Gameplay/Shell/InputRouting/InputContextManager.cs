using System;
using R3;
using UnityEngine.InputSystem;
using Roguelike.Application.Enums;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Shered;

namespace Roguelike.Presentation.Gameplay.Shell.InputRouting
{
    /// <summary>
    /// 現在のゲーム状態/UI状態から入力コンテキストを解決し、
    /// 対応するInputActionMapへ切り替える管理クラスです。
    /// </summary>
    public sealed class InputContextManager : IDisposable
    {
        private readonly RunResultStore _runResultStore;
        private readonly GetCurrentRunPhaseQueryHandler _getCurrentRunPhaseQueryHandler;
        private readonly RunUiController _uiController;
        private readonly CompositeDisposable _disposables = new();
        private readonly InputSystem_Actions _inputActions;
        private readonly InputActionMap[] _contextMaps;

        private bool _initialized;
        private InputActionMap _activeContextMap;

        /// <summary>現在有効な入力コンテキスト。</summary>
        public ReactiveProperty<RunInputContext> CurrentContext { get; }

        /// <summary>
        /// すべての入力ハンドラで共有するInputActionsインスタンスです。
        /// </summary>
        public InputSystem_Actions InputActions => _inputActions;

        public InputContextManager(
            RunResultStore runResultStore,
            GetCurrentRunPhaseQueryHandler getCurrentRunPhaseQueryUseCase,
            RunUiController uiController)
        {
            _runResultStore = runResultStore;
            _getCurrentRunPhaseQueryHandler = getCurrentRunPhaseQueryUseCase;
            _uiController = uiController;

            _inputActions = new InputSystem_Actions();
            _contextMaps = new[]
            {
                _inputActions.Player.Get(),
                _inputActions.MainMenu.Get(),
                _inputActions.Inventory.Get(),
                _inputActions.Operation.Get(),
                _inputActions.Confirm.Get()
            };

            DisableContextMaps();
            _inputActions.UI.Enable();
            _inputActions.DeviceInput.Enable();

            CurrentContext = new ReactiveProperty<RunInputContext>(RunInputContext.Blocked)
                .AddTo(_disposables);
        }

        /// <summary>
        /// 状態監視を開始し、現在コンテキストを適用します。
        /// </summary>
        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _runResultStore?.HasResult?
                .Subscribe(_ => RefreshContext())
                .AddTo(_disposables);

            _uiController?.CurrentState?
                .Subscribe(_ => RefreshContext())
                .AddTo(_disposables);

            // 起動時に必ず一度コンテキスト解決を行う。
            RefreshContext(force: true);
        }

        private void RefreshContext(bool force = false)
        {
            var next = ResolveContext();
            if (!force && next == CurrentContext.Value)
            {
                return;
            }

            CurrentContext.Value = next;
            ApplyActionMap(next);
        }

        private void ApplyActionMap(RunInputContext context)
        {
            var nextMap = ResolveContextMap(context);
            if (ReferenceEquals(nextMap, _activeContextMap))
            {
                return;
            }

            // 複数Map同時有効を避けるため、毎回全解除してから1つだけ有効化する。
            DisableContextMaps();

            if (nextMap != null)
            {
                nextMap.Enable();
            }

            _activeContextMap = nextMap;
        }

        private InputActionMap ResolveContextMap(RunInputContext context)
        {
            return context switch
            {
                // メニュー系は専用ActionMapを割り当てる。
                RunInputContext.Exploration => _inputActions.Player.Get(),
                RunInputContext.Menu => _inputActions.MainMenu.Get(),
                RunInputContext.Inventory => _inputActions.Inventory.Get(),
                RunInputContext.FloorConfirm => _inputActions.Confirm.Get(),
                RunInputContext.SpellPreview => _inputActions.Confirm.Get(),
                RunInputContext.Guide => _inputActions.Operation.Get(),
                RunInputContext.Result => _inputActions.Confirm.Get(),
                RunInputContext.Pause => null,
                RunInputContext.Blocked => null,
                _ => null
            };
        }

        private void DisableContextMaps()
        {
            for (var i = 0; i < _contextMaps.Length; i++)
            {
                var map = _contextMaps[i];
                if (map.enabled)
                {
                    map.Disable();
                }
            }

            _activeContextMap = null;
        }

        private RunInputContext ResolveContext()
        {
            var hasResult = _runResultStore?.HasResult?.CurrentValue ?? false;
            if (hasResult)
            {
                // 結果表示は最優先で固定。
                return RunInputContext.Result;
            }

            var runPhaseResult = _getCurrentRunPhaseQueryHandler?.Handle(new GetCurrentRunPhaseQuery());
            var hasPhase = runPhaseResult.HasValue && runPhaseResult.Value.IsSuccess;
            var runPhase = hasPhase ? runPhaseResult.Value.Value : RunPhaseDto.None;
            if (hasPhase && (runPhase == RunPhaseDto.Clear || runPhase == RunPhaseDto.GameOver))
            {
                return RunInputContext.Result;
            }

            var uiState = _uiController?.CurrentState?.Value ?? RunUiState.None;
            return uiState switch
            {
                RunUiState.None => ResolveNoneStateContext(runPhase),
                RunUiState.Menu => RunInputContext.Menu,
                RunUiState.Inventory or RunUiState.InventoryDetail or RunUiState.InventoryDescription
                    => RunInputContext.Inventory,
                RunUiState.FloorConfirm => RunInputContext.FloorConfirm,
                RunUiState.SpellPreview => RunInputContext.SpellPreview,
                RunUiState.Guide => RunInputContext.Guide,
                _ => RunInputContext.Blocked
            };
        }

        private static RunInputContext ResolveNoneStateContext(RunPhaseDto runPhase)
        {
            if (runPhase == RunPhaseDto.Pause)
            {
                return RunInputContext.Pause;
            }

            if (runPhase == RunPhaseDto.InRun)
            {
                return RunInputContext.Exploration;
            }

            return RunInputContext.Blocked;
        }

        public void Dispose()
        {
            _disposables.Dispose();

            DisableContextMaps();

            if (_inputActions.UI.enabled)
            {
                _inputActions.UI.Disable();
            }

            if (_inputActions.DeviceInput.enabled)
            {
                _inputActions.DeviceInput.Disable();
            }

            _inputActions.Dispose();
        }
    }
}



