using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Roguelike.Presentation.Gameplay.FloorTransition.Presenters;
using Roguelike.Presentation.Gameplay.Guide.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;

namespace Roguelike.Presentation.Gameplay.RunResult.Input
{
    /// <summary>
    /// 結果画面での入力を処理するハンドラーです。
    /// </summary>
    public sealed class RunResultInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly RunResultStore _runResultStore;
        [Inject] private readonly IGameplayResultNavigation _resultNavigation;

        private readonly CompositeDisposable _disposables = new();
        private bool _canAcceptInput;
        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.Result;
        }

        public void Init()
        {
            if (_runResultStore == null)
            {
                Debug.LogWarning("RunResultStore is not injected.");
                return;
            }

            _runResultStore.HasResult
                .Subscribe(OnHasResultChanged)
                .AddTo(_disposables);

            Enable();
        }

        public void Enable()
        {
            if (_isEnabled)
            {
                return;
            }

            if (_inputContextManager == null)
            {
                Debug.LogWarning("InputContextManager is not injected.");
                return;
            }

            SubscribeInput();
            _isEnabled = true;
        }

        public void Disable()
        {
            if (!_isEnabled)
            {
                return;
            }

            var confirmActions = _inputContextManager.InputActions.Confirm;

            confirmActions.UseItem.performed -= OnConfirmPerformed;
            confirmActions.Submit.performed -= OnConfirmPerformed;

            _isEnabled = false;
        }

        private void OnHasResultChanged(bool hasResult)
        {
            _canAcceptInput = hasResult;
        }

        private void SubscribeInput()
        {
            var confirmActions = _inputContextManager.InputActions.Confirm;

            confirmActions.UseItem.performed -= OnConfirmPerformed;
            confirmActions.UseItem.performed += OnConfirmPerformed;

            confirmActions.Submit.performed -= OnConfirmPerformed;
            confirmActions.Submit.performed += OnConfirmPerformed;
        }

        private void OnConfirmPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextResult())
            {
                return;
            }

            if (!_canAcceptInput)
            {
                return;
            }

            GoToTitle();
        }

        /// <summary>
        /// タイトル（新規ラン）へ遷移します。
        /// ボタンクリックまたは入力から呼び出されます。
        /// </summary>
        public void GoToTitle()
        {
            if (!_canAcceptInput)
            {
                return;
            }

            if (!IsCurrentContextResult())
            {
                return;
            }

            _canAcceptInput = false;
            _resultNavigation?.TryGoToTitle();
        }

        private bool IsCurrentContextResult()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.Result;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            Disable();
        }
    }
}




