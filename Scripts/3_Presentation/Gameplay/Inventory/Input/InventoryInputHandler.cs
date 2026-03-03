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

namespace Roguelike.Presentation.Gameplay.Inventory.Input
{
    /// <summary>
    /// インベントリ画面での入力を担当します。
    /// </summary>
    public sealed class InventoryInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly InventoryPresenter _inventoryController;
        [Inject] private readonly RunUiController _runUiController;

        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.Inventory;
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

            var actions = _inputContextManager.InputActions.Inventory;

            actions.Next.performed -= OnNextPerformed;
            actions.Previous.performed -= OnPreviousPerformed;
            actions.UseItem.performed -= OnUseItemPerformed;
            actions.Cancel.performed -= OnCancel;
            actions.NextPage.performed -= OnNextPagePerformed;
            actions.PreviousPage.performed -= OnPreviousPagePerformed;

            _isEnabled = false;
        }

        private void SubscribeInput()
        {
            var actions = _inputContextManager.InputActions.Inventory;

            actions.Next.performed -= OnNextPerformed;
            actions.Next.performed += OnNextPerformed;

            actions.Previous.performed -= OnPreviousPerformed;
            actions.Previous.performed += OnPreviousPerformed;

            actions.UseItem.performed -= OnUseItemPerformed;
            actions.UseItem.performed += OnUseItemPerformed;

            actions.Cancel.performed -= OnCancel;
            actions.Cancel.performed += OnCancel;

            actions.NextPage.performed -= OnNextPagePerformed;
            actions.NextPage.performed += OnNextPagePerformed;

            actions.PreviousPage.performed -= OnPreviousPagePerformed;
            actions.PreviousPage.performed += OnPreviousPagePerformed;
        }

        private void OnNextPagePerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsDetailMenuOpen()) return;
            if (IsDescriptionViewOpen()) return;
            if (IsSpellPreviewOpen()) return;
            _inventoryController.NextPage();
        }

        private void OnPreviousPagePerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsDetailMenuOpen()) return;
            if (IsDescriptionViewOpen()) return;
            if (IsSpellPreviewOpen()) return;
            _inventoryController.PreviousPage();
        }

        private void OnNextPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsDescriptionViewOpen()) return;
            if (IsSpellPreviewOpen()) return;

            if (IsDetailMenuOpen())
            {
                _inventoryController.DetailMenuSelectNext();
                return;
            }

            _inventoryController.SelectNext();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsDescriptionViewOpen()) return;
            if (IsSpellPreviewOpen()) return;

            if (IsDetailMenuOpen())
            {
                _inventoryController.DetailMenuSelectPrevious();
                return;
            }

            _inventoryController.SelectPrevious();
        }

        private void OnUseItemPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsDescriptionViewOpen()) return;
            if (IsSpellPreviewOpen()) return;

            var selectedItem = _inventoryController.GetSelectedItem();
            if (selectedItem == null) return;

            if (IsDetailMenuOpen())
            {
                // 詳細メニュー表示中は「確定＝選択中オプション実行」。
                _inventoryController.ExecuteSelectedDetailOption();
                return;
            }

            // 一覧表示中は「確定＝詳細メニューを開く」。
            _inventoryController.OpenDetailMenu();
        }

        private void OnCancel(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextInventory()) return;
            if (!IsInventoryOpen()) return;
            if (IsSpellPreviewOpen()) return;

            if (IsDescriptionViewOpen())
            {
                _inventoryController.CloseDescriptionView();
            }
            else if (IsDetailMenuOpen())
            {
                _inventoryController.CloseDetailMenu();
            }
            else
            {
                _inventoryController.CloseInventory();
            }
        }

        private bool IsInventoryOpen()
        {
            return _runUiController != null && _runUiController.IsInventoryOpen.CurrentValue;
        }

        private bool IsDetailMenuOpen()
        {
            return _runUiController != null && _runUiController.IsDetailMenuOpen.CurrentValue;
        }

        private bool IsDescriptionViewOpen()
        {
            return _runUiController != null && _runUiController.IsDescriptionViewOpen.CurrentValue;
        }

        private bool IsSpellPreviewOpen()
        {
            return _runUiController != null && _runUiController.IsSpellPreviewOpen.CurrentValue;
        }

        private bool IsCurrentContextInventory()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.Inventory;
        }

        /// <summary>
        /// ドロップ処理（Dキー）。
        /// ExplorationInputHandlerから呼び出される想定。
        /// </summary>
        public void HandleDropItem()
        {
            // 実際のドロップ判定/行動実行はController側に集約。
            _inventoryController.DropSelectedItemFromShortcut();
        }

        public void Dispose()
        {
            Disable();
        }
    }
}





