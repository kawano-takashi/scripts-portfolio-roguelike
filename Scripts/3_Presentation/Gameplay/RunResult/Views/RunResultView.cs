using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;

namespace Roguelike.Presentation.Gameplay.RunResult.Views
{
    /// <summary>
    /// クリア/ゲームオーバーの結果画面を表示する受動ビューです。
    /// </summary>
    public sealed class RunResultView : MonoBehaviour, IRunResultView
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _floorText;
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _toTitleButton;

        [Header("Colors")]
        [SerializeField] private Color _clearColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color _gameOverColor = new Color(0.9f, 0.2f, 0.2f);

        [Header("Animation Timing")]
        [SerializeField, Range(0f, 2f)] private float _showDelaySeconds = 0.5f;
        [SerializeField, Range(0f, 3f)] private float _fadeDurationSeconds = 1f;

        private CancellationTokenSource _showCts;

        public event Action GoToTitleRequested;

        public void Init()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            if (_toTitleButton != null)
            {
                _toTitleButton.SetActive(false);
            }
        }

        public void Render(RunResultDisplayModel model)
        {
            if (model == null)
            {
                Hide();
                return;
            }

            CancelRunningAnimation();
            _showCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ShowResultAsync(model, _showCts.Token).Forget();
        }

        public void Hide()
        {
            CancelRunningAnimation();

            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            if (_toTitleButton != null)
            {
                _toTitleButton.SetActive(false);
            }
        }

        public void GoToTitle()
        {
            GoToTitleRequested?.Invoke();
        }

        private async UniTaskVoid ShowResultAsync(RunResultDisplayModel resultData, CancellationToken token)
        {
            if (_showDelaySeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_showDelaySeconds), cancellationToken: token);
            }

            UpdateContent(resultData);

            if (_panel != null)
            {
                _panel.SetActive(true);
            }

            if (_toTitleButton != null)
            {
                _toTitleButton.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                var tween = _canvasGroup.DOFade(1f, _fadeDurationSeconds).SetEase(Ease.InOutSine);
                var duration = tween.Duration();
                if (duration > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
                }
            }

            if (_toTitleButton != null)
            {
                _toTitleButton.SetActive(true);
            }
        }

        private void UpdateContent(RunResultDisplayModel data)
        {
            if (_titleText != null)
            {
                _titleText.text = data.IsVictory ? "DUNGEON CLEARED!" : "GAME OVER";
                _titleText.color = data.IsVictory ? _clearColor : _gameOverColor;
            }

            if (_floorText != null)
            {
                _floorText.text = $"{data.FinalFloor}F";
            }

            if (_turnText != null)
            {
                _turnText.text = $"{data.TotalTurns}";
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{data.PlayerLevel}";
            }
        }

        private void CancelRunningAnimation()
        {
            if (_showCts == null)
            {
                return;
            }

            _showCts.Cancel();
            _showCts.Dispose();
            _showCts = null;
        }

        private void OnDestroy()
        {
            CancelRunningAnimation();
        }
    }
}




