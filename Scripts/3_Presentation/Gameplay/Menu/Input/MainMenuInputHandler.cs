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

namespace Roguelike.Presentation.Gameplay.Menu.Input
{
    /// <summary>
    /// メインメニュー表示中の入力を処理します。
    /// 入力判定後の実処理は <see cref="MainMenuPresenter"/> に委譲します。
    /// </summary>
    public sealed class MainMenuInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly MainMenuPresenter _mainMenuController;
        [Inject] private readonly RunUiController _runUiController;

        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.Menu;
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

            var actions = _inputContextManager.InputActions.MainMenu;

            actions.Next.performed -= OnNextPerformed;
            actions.Previous.performed -= OnPreviousPerformed;
            actions.UseItem.performed -= OnConfirmPerformed;
            actions.Cancel.performed -= OnCancelPerformed;

            _isEnabled = false;
        }

        private void SubscribeInput()
        {
            var actions = _inputContextManager.InputActions.MainMenu;

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
            if (!IsCurrentContextMenu())
            {
                return;
            }

            if (!IsMenuOpen())
            {
                return;
            }

            // ハンドラは入力解釈のみを担当し、状態更新はControllerに任せる。
            _mainMenuController.SelectNext();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextMenu())
            {
                return;
            }

            if (!IsMenuOpen())
            {
                return;
            }

            _mainMenuController.SelectPrevious();
        }

        private void OnConfirmPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextMenu())
            {
                return;
            }

            if (!IsMenuOpen())
            {
                return;
            }

            _mainMenuController.ConfirmSelection();
        }

        private void OnCancelPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextMenu())
            {
                return;
            }

            if (!IsMenuOpen())
            {
                return;
            }

            _mainMenuController.CloseMenu();
        }

        private bool IsMenuOpen()
        {
            return _runUiController != null && _runUiController.IsMenuOpen.CurrentValue;
        }

        private bool IsCurrentContextMenu()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.Menu;
        }

        public void Dispose()
        {
            Disable();
        }
    }
}





