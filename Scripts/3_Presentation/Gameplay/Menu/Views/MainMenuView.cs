using UnityEngine;
using R3;
using VContainer;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Types;

namespace Roguelike.Presentation.Gameplay.Menu.Views
{
    /// <summary>
    /// メインメニューパネルの表示更新を担当します。
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private MainMenuSlotView[] _slotViews;
        [SerializeField] private Color _selectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Inject] private readonly MainMenuPresenter _mainMenuController;
        [Inject] private readonly RunUiController _runUiController;

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            if (_mainMenuController == null || _runUiController == null)
            {
                Debug.LogError("MainMenuPresenter or RunUiController is not injected!");
                return;
            }

            if (_menuPanel != null)
            {
                _menuPanel.SetActive(false);
            }

            InitializeSlots();

            _runUiController.IsMenuOpen
                .ObserveOnMainThread()
                .Subscribe(OnMenuOpenChanged)
                .AddTo(_disposables);

            // 選択状態はControllerを監視して反映する。
            _mainMenuController.SelectedIndex
                .ObserveOnMainThread()
                .Subscribe(_ => UpdateSelection())
                .AddTo(_disposables);
        }

        private void InitializeSlots()
        {
            if (_slotViews == null || _slotViews.Length == 0)
            {
                return;
            }

            var options = _mainMenuController.MenuOptions;
            for (var i = 0; i < _slotViews.Length && i < options.Count; i++)
            {
                var slot = _slotViews[i];
                if (slot == null) continue;

                var option = options[i];
                slot.Bind(option, GetDisplayText(option));
            }
        }

        private string GetDisplayText(MainMenuOption option)
        {
            return option switch
            {
                MainMenuOption.Inventory => "アイテム",
                MainMenuOption.Status => "ステータス",
                MainMenuOption.Settings => "設定",
                MainMenuOption.OperationGuide => "操作説明",
                MainMenuOption.Close => "閉じる",
                _ => option.ToString()
            };
        }

        private void OnMenuOpenChanged(bool isOpen)
        {
            if (_menuPanel != null)
            {
                _menuPanel.SetActive(isOpen);
            }

            if (isOpen)
            {
                UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            if (_slotViews == null || _slotViews.Length == 0)
            {
                return;
            }

            var selectedIndex = _mainMenuController.SelectedIndex.Value;
            var options = _mainMenuController.MenuOptions;

            for (var i = 0; i < _slotViews.Length && i < options.Count; i++)
            {
                var slot = _slotViews[i];
                if (slot == null) continue;

                var option = options[i];
                var isEnabled = _mainMenuController.IsOptionEnabled(option);
                var isSelected = i == selectedIndex;

                if (isSelected)
                {
                    slot.SetColor(isEnabled ? _selectedColor : _disabledColor);
                }
                else
                {
                    slot.SetColor(isEnabled ? _normalColor : _disabledColor);
                }
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}





