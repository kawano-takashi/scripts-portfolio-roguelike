using UnityEngine;
using TMPro;
using R3;
using VContainer;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Guide.Presenters;

namespace Roguelike.Presentation.Gameplay.Guide.Views
{
    /// <summary>
    /// 操作説明パネルの表示を担当します。
    /// </summary>
    public sealed class OperationGuideView : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [TextArea(3, 12)]
        [SerializeField] private string _defaultText;

        [Inject] private readonly OperationGuidePresenter _operationGuideController;
        [Inject] private readonly RunUiController _runUiController;

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            if (_operationGuideController == null || _runUiController == null)
            {
                Debug.LogError("OperationGuidePresenter or RunUiController is not injected!");
                return;
            }

            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            _runUiController.IsGuideOpen
                .ObserveOnMainThread()
                .Subscribe(OnOpenChanged)
                .AddTo(_disposables);
        }

        private void OnOpenChanged(bool isOpen)
        {
            if (_panel != null)
            {
                _panel.SetActive(isOpen);
            }

            if (isOpen && _bodyText != null && !string.IsNullOrEmpty(_defaultText))
            {
                // テキストの中身自体はView責務としてInspector設定を表示する。
                _bodyText.text = _defaultText;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}





