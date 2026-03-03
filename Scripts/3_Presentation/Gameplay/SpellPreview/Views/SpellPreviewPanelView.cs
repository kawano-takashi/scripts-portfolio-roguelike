using UnityEngine;
using TMPro;
using R3;
using VContainer;
using Roguelike.Presentation.Gameplay.Shell.Core;

namespace Roguelike.Presentation.Gameplay.SpellPreview.Views
{
    /// <summary>
    /// 呪文プレビュー終了確認パネルの表示を担当します。
    /// </summary>
    public sealed class SpellPreviewPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _noText;
        [SerializeField] private Color _selectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        [Inject] private readonly RunUiController _runUiController;

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            if (_runUiController == null)
            {
                Debug.LogError("RunUiController is not injected!");
                return;
            }

            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            _runUiController.IsSpellPreviewOpen
                .ObserveOnMainThread()
                .Subscribe(OnPreviewOpenChanged)
                .AddTo(_disposables);
        }

        private void OnPreviewOpenChanged(bool isOpen)
        {
            if (_panel != null)
            {
                _panel.SetActive(isOpen);
            }

            if (isOpen)
            {
                ApplySingleOptionStyle();
            }
        }

        private void ApplySingleOptionStyle()
        {
            if (_noText == null)
            {
                return;
            }

            _noText.color = _selectedColor;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
