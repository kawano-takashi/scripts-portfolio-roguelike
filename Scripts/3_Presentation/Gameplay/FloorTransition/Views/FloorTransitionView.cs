using UnityEngine;
using TMPro;
using R3;
using VContainer;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.FloorTransition.Presenters;

namespace Roguelike.Presentation.Gameplay.FloorTransition.Views
{
    /// <summary>
    /// フロア遷移確認パネルの表示更新を担当します。
    /// </summary>
    public sealed class FloorTransitionView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _yesText;
        [SerializeField] private TextMeshProUGUI _noText;
        [SerializeField] private Color _selectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        [Inject] private readonly FloorTransitionPresenter _floorTransitionController;
        [Inject] private readonly RunUiController _runUiController;

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            if (_floorTransitionController == null || _runUiController == null)
            {
                Debug.LogError("FloorTransitionPresenter or RunUiController is not injected!");
                return;
            }

            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            _runUiController.IsFloorConfirmOpen
                .ObserveOnMainThread()
                .Subscribe(OnConfirmOpenChanged)
                .AddTo(_disposables);

            _floorTransitionController.SelectedIndex
                .ObserveOnMainThread()
                .Subscribe(_ => UpdateSelection())
                .AddTo(_disposables);
        }

        private void OnConfirmOpenChanged(bool isOpen)
        {
            if (_panel != null)
            {
                _panel.SetActive(isOpen);
            }

            if (isOpen)
            {
                UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            if (_yesText == null || _noText == null)
            {
                return;
            }

            var selectedIndex = _floorTransitionController.SelectedIndex.Value;
            _yesText.color = selectedIndex == 0 ? _selectedColor : _normalColor;
            _noText.color = selectedIndex == 1 ? _selectedColor : _normalColor;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}





