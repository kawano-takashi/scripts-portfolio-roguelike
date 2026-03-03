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

namespace Roguelike.Presentation.Gameplay.Guide.Input
{
    /// <summary>
    /// 操作説明パネル表示中の入力を担当します。
    /// </summary>
    public sealed class OperationGuideInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly OperationGuidePresenter _operationGuideController;

        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.Guide;
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

            var cancel = _inputContextManager?.InputActions?.Operation.Cancel;
            if (cancel != null)
            {
                cancel.performed -= OnCancelPerformed;
            }

            _isEnabled = false;
        }

        private void SubscribeInput()
        {
            var cancel = _inputContextManager?.InputActions?.Operation.Cancel;
            if (cancel == null)
            {
                return;
            }

            cancel.performed -= OnCancelPerformed;
            cancel.performed += OnCancelPerformed;
        }

        private void OnCancelPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextGuide())
            {
                return;
            }

            _operationGuideController?.Close();
        }

        private bool IsCurrentContextGuide()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.Guide;
        }

        public void Dispose()
        {
            Disable();
        }
    }
}





