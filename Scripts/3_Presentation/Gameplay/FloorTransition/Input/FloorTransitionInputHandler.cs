using System;
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

namespace Roguelike.Presentation.Gameplay.FloorTransition.Input
{
    /// <summary>
    /// フロア遷移確認ダイアログ表示中の入力を処理します。
    /// </summary>
    public sealed class FloorTransitionInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly FloorTransitionPresenter _floorTransitionController;
        [Inject] private readonly RunUiController _runUiController;

        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.FloorConfirm;
        }

        public void Init()
        {
            if (_inputContextManager == null)
            {
                Debug.LogError("InputContextManager is not injected!");
                return;
            }

            Enable();
        }

        public void Enable()
        {
            if (_isEnabled)
            {
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

            var actions = _inputContextManager.InputActions.Confirm;
            actions.Next.performed -= OnNextPerformed;
            actions.Previous.performed -= OnPreviousPerformed;
            actions.UseItem.performed -= OnConfirmPerformed;
            actions.Cancel.performed -= OnCancelPerformed;

            _isEnabled = false;
        }

        private void SubscribeInput()
        {
            var actions = _inputContextManager.InputActions.Confirm;

            actions.Next.performed -= OnNextPerformed;
            actions.Next.performed += OnNextPerformed;

            actions.Previous.performed -= OnPreviousPerformed;
            actions.Previous.performed += OnPreviousPerformed;

            actions.UseItem.performed -= OnConfirmPerformed;
            actions.UseItem.performed += OnConfirmPerformed;

            actions.Cancel.performed -= OnCancelPerformed;
            actions.Cancel.performed += OnCancelPerformed;
        }

        private void OnNextPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextFloorConfirm())
            {
                return;
            }

            if (!IsConfirmOpen())
            {
                return;
            }

            _floorTransitionController.SelectNext();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextFloorConfirm())
            {
                return;
            }

            if (!IsConfirmOpen())
            {
                return;
            }

            _floorTransitionController.SelectPrevious();
        }

        private void OnConfirmPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextFloorConfirm())
            {
                return;
            }

            if (!IsConfirmOpen())
            {
                return;
            }

            _floorTransitionController.ConfirmSelection();
        }

        private void OnCancelPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextFloorConfirm())
            {
                return;
            }

            if (!IsConfirmOpen())
            {
                return;
            }

            _floorTransitionController.Cancel();
        }

        private bool IsConfirmOpen()
        {
            return _runUiController != null && _runUiController.IsFloorConfirmOpen.CurrentValue;
        }

        private bool IsCurrentContextFloorConfirm()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.FloorConfirm;
        }

        public void Dispose()
        {
            Disable();
        }
    }
}





