using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using VContainer;
using Roguelike.Application.Dtos;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;

namespace Roguelike.Presentation.Gameplay.Inventory.Views
{
    /// <summary>
    /// インベントリ画面の表示を担当します。
    /// </summary>
    public sealed class InventoryView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private InventorySlotView[] _itemSlots = new InventorySlotView[10];
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _pageIndicatorText;

        [Header("Detail Menu")]
        [SerializeField] private GameObject _detailMenuPanel;
        [SerializeField] private Transform _detailMenuContainer;
        [SerializeField] private GameObject _menuOptionPrefab;

        [Header("Description Panel")]
        [SerializeField] private GameObject _descriptionPanel;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Inject] private readonly InventoryPresenter _inventoryController;
        [Inject] private readonly RunUiController _runUiController;

        private readonly List<GameObject> _menuOptionInstances = new();
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// 準備をして、インベントリ表示を開始します。
        /// </summary>
        public void Init()
        {
            if (_inventoryController == null || _runUiController == null)
            {
                Debug.LogError("InventoryPresenter or RunUiController is not injected!");
                return;
            }

            // 最初は非表示
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(false);
            }
            if (_detailMenuPanel != null)
            {
                _detailMenuPanel.SetActive(false);
            }
            if (_descriptionPanel != null)
            {
                _descriptionPanel.SetActive(false);
            }

            // インベントリ開閉を監視
            _runUiController.IsInventoryOpen
                .Subscribe(OnInventoryOpenChanged)
                .AddTo(_disposables);

            // アイテム一覧の変更を監視
            _inventoryController.InventoryItems
                .Subscribe(_ => UpdateSlotsForCurrentPage())
                .AddTo(_disposables);

            // 選択位置の変更を監視
            _inventoryController.SelectedIndex
                .Subscribe(_ => UpdateSelection())
                .AddTo(_disposables);

            // ページ変更を監視
            _inventoryController.CurrentPageIndex
                .Subscribe(_ => UpdateSlotsForCurrentPage())
                .AddTo(_disposables);

            // 詳細メニュー開閉を監視
            _runUiController.IsDetailMenuOpen
                .Subscribe(OnDetailMenuOpenChanged)
                .AddTo(_disposables);

            // 詳細メニュー選択位置を監視
            _inventoryController.DetailMenuSelectedIndex
                .Subscribe(_ => UpdateDetailMenuSelection())
                .AddTo(_disposables);

            // 詳細メニュー選択肢を監視
            _inventoryController.DetailMenuOptions
                .Subscribe(_ =>
                {
                    if (_runUiController.IsDetailMenuOpen.CurrentValue)
                    {
                        BuildDetailMenu();
                        UpdateDetailMenuSelection();
                    }
                })
                .AddTo(_disposables);

            // 説明パネル開閉を監視
            _runUiController.IsDescriptionViewOpen
                .Subscribe(OnDescriptionViewOpenChanged)
                .AddTo(_disposables);
        }

        private void OnInventoryOpenChanged(bool isOpen)
        {
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(isOpen);
            }

            if (isOpen)
            {
                UpdateSlotsForCurrentPage();
                UpdateSelection();
            }
        }

        private void UpdateSlotsForCurrentPage()
        {
            var items = _inventoryController.InventoryItems.Value;
            var pageIndex = _inventoryController.CurrentPageIndex.Value;
            var startIndex = pageIndex * InventoryPresenter.ItemsPerPage;

            for (int i = 0; i < _itemSlots.Length; i++)
            {
                var globalIndex = startIndex + i;
                var slot = _itemSlots[i];
                if (slot == null) continue;

                if (items != null && globalIndex < items.Count)
                {
                    var item = items[globalIndex];
                    slot.SetItem(_inventoryController.GetItemDisplayName(item));
                }
                else
                {
                    slot.SetEmpty();
                }
            }

            UpdateTitle(items?.Count ?? 0);
            UpdatePageIndicator();
            UpdateSelection();
        }

        private void UpdateTitle(int count)
        {
            if (_titleText != null)
            {
                var capacity = InventoryPresenter.MaxItems;
                _titleText.text = $"INVENTORY ({count}/{capacity})";
            }
        }

        private void UpdatePageIndicator()
        {
            if (_pageIndicatorText != null)
            {
                var currentPage = _inventoryController.CurrentPageIndex.Value + 1;
                _pageIndicatorText.text = $"{currentPage}/{InventoryPresenter.TotalPages}";
            }
        }

        private void UpdateSelection()
        {
            var selectedIndex = _inventoryController.SelectedIndex.Value;
            var pageIndex = _inventoryController.CurrentPageIndex.Value;
            var pageLocalIndex = selectedIndex - (pageIndex * InventoryPresenter.ItemsPerPage);

            for (int i = 0; i < _itemSlots.Length; i++)
            {
                var slot = _itemSlots[i];
                if (slot == null) continue;

                slot.SetSelected(i == pageLocalIndex);
            }
        }

        private void OnDetailMenuOpenChanged(bool isOpen)
        {
            if (_detailMenuPanel != null)
            {
                _detailMenuPanel.SetActive(isOpen);
            }

            if (isOpen)
            {
                BuildDetailMenu();
                UpdateDetailMenuSelection();
            }
        }

        private void BuildDetailMenu()
        {
            // 既存のメニュー項目をクリア
            foreach (var option in _menuOptionInstances)
            {
                if (option != null)
                {
                    Destroy(option);
                }
            }
            _menuOptionInstances.Clear();

            if (_detailMenuContainer == null || _menuOptionPrefab == null) return;

            var selectedItem = _inventoryController?.GetSelectedItem();
            var options = _inventoryController?.DetailMenuOptions?.Value;
            if (options == null || options.Count == 0)
            {
                return;
            }

            // メニュー項目を作成
            for (int i = 0; i < options.Count; i++)
            {
                var optionGo = Instantiate(_menuOptionPrefab, _detailMenuContainer);
                _menuOptionInstances.Add(optionGo);

                var textComponent = optionGo.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = _inventoryController.GetDetailMenuLabel(
                        options[i],
                        selectedItem.GetValueOrDefault());
                }
            }
        }

        private void UpdateDetailMenuSelection()
        {
            if (!_runUiController.IsDetailMenuOpen.CurrentValue) return;

            var selectedIndex = _inventoryController.DetailMenuSelectedIndex.Value;

            for (int i = 0; i < _menuOptionInstances.Count; i++)
            {
                var option = _menuOptionInstances[i];
                if (option == null) continue;

                var image = option.GetComponent<Image>();
                var textComponent = option.GetComponentInChildren<TextMeshProUGUI>();
                if (image != null)
                {
                    image.color = (i == selectedIndex) ? Color.yellow : Color.clear;
                }

                if (textComponent != null)
                {
                    textComponent.color = (i == selectedIndex) ? Color.black : Color.white;
                }
            }
        }

        private void OnDescriptionViewOpenChanged(bool isOpen)
        {
            // 説明パネルの表示/非表示を切り替え
            if (_descriptionPanel != null)
            {
                _descriptionPanel.SetActive(isOpen);
            }

            // 説明パネルが開いた場合は詳細説明テキストを更新
            if (isOpen && _descriptionText != null)
            {
                var selectedItem = _inventoryController.GetSelectedItem();
                if (selectedItem.HasValue)
                {
                    _descriptionText.text = _inventoryController.GetDetailedItemDescription(selectedItem.Value);
                }
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();

            foreach (var option in _menuOptionInstances)
            {
                if (option != null)
                {
                    Destroy(option);
                }
            }
            _menuOptionInstances.Clear();
        }
    }
}





