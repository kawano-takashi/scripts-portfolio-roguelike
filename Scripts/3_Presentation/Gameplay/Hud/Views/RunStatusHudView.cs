using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Presentation.Gameplay.Hud.Stores;

namespace Roguelike.Presentation.Gameplay.Hud.Views
{
    public sealed class RunStatusHudView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _floorText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _hungerText;

        [Header("Gauges")]
        [SerializeField] private Slider _hpGauge;
        [SerializeField] private Slider _hungerGauge;

        [Inject] private RunStatusStore _runStatusStore;

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            if (_runStatusStore == null)
            {
                return;
            }

            _runStatusStore.Refresh();
            UpdateAll(_runStatusStore.CurrentStatus.Value);

            _runStatusStore.CurrentStatus
                .ObserveOnMainThread()
                .Subscribe(UpdateAll)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void UpdateAll(RunSnapshotDto status)
        {
            UpdateFloor(status);
            UpdateLevel(status);
            UpdateHp(status);
            UpdateHunger(status);
        }

        private void UpdateFloor(RunSnapshotDto status)
        {
            if (_floorText == null)
            {
                return;
            }

            _floorText.text = $"F{status.Floor}";
        }

        private void UpdateLevel(RunSnapshotDto status)
        {
            if (_levelText == null)
            {
                return;
            }

            _levelText.text = $"Lv {status.PlayerLevel}";
        }

        private void UpdateHp(RunSnapshotDto status)
        {
            var current = status.PlayerCurrentHp;
            var max = status.PlayerMaxHp;

            if (_hpText != null)
            {
                _hpText.text = $"HP {current}/{max}";
            }

            if (_hpGauge != null)
            {
                _hpGauge.minValue = 0f;
                _hpGauge.maxValue = max;
                _hpGauge.value = Mathf.Clamp(current, 0, max);
            }
        }

        private void UpdateHunger(RunSnapshotDto status)
        {
            var current = status.PlayerCurrentHunger;
            var max = status.PlayerMaxHunger;

            if (_hungerText != null)
            {
                // 値を切り上げる
                var displayCurrent = Mathf.CeilToInt(current);
                var displayMax = Mathf.CeilToInt(max);
                _hungerText.text = $"空腹度 {displayCurrent}/{displayMax}";
            }

            if (_hungerGauge != null)
            {
                _hungerGauge.minValue = 0f;
                _hungerGauge.maxValue = max;
                _hungerGauge.value = Mathf.Clamp(current, 0, max);
            }
        }
    }
}




