using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;

namespace Roguelike.Presentation.Gameplay.SpellPreview.Input
{
    /// <summary>
    /// 呪文プレビュー表示中の入力を担当します。
    /// </summary>
    public sealed class SpellPreviewInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly SpellPreviewPresenter _spellPreviewController;
        [Inject] private readonly RunUiController _runUiController;

        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.SpellPreview;
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
            actions.UseItem.performed -= OnConfirmPerformed;

            _isEnabled = false;
        }

        private void SubscribeInput()
        {
            var actions = _inputContextManager.InputActions.Confirm;

            actions.UseItem.performed -= OnConfirmPerformed;
            actions.UseItem.performed += OnConfirmPerformed;
        }

        private void OnConfirmPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextSpellPreview())
            {
                return;
            }

            if (!IsSpellPreviewOpen())
            {
                return;
            }

            // 呪文プレビューを閉じます。
            _spellPreviewController.ConfirmSelection();
        }

        private bool IsSpellPreviewOpen()
        {
            return _runUiController != null && _runUiController.IsSpellPreviewOpen.CurrentValue;
        }

        private bool IsCurrentContextSpellPreview()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.SpellPreview;
        }

        public void Dispose()
        {
            Disable();
        }
    }
}
