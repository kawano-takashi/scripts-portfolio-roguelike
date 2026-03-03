using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using VContainer;
using R3;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.Hud.Views
{
    /// <summary>
    /// ランログを画面に表示するビューです。
    /// イベントごとにログを表示します。
    /// </summary>
    public sealed class RunLogView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _contentParent;

        [Header("Prefab")]
        [SerializeField] private GameObject _logEntryPrefab;

        [Header("Settings")]
        [SerializeField, Range(1, 50), FormerlySerializedAs("_maxTurnLogs"), FormerlySerializedAs("_maxLogEntries")]
        private int _maxLogEntries = 10;

        [Header("Colors")]
        [SerializeField] private Color _damageColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _defeatColor = new Color(0.8f, 0f, 0f);
        [SerializeField] private Color _healColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color _systemColor = new Color(0.7f, 0.7f, 0.7f);

        [Inject] private RunLogStore _runLogStore;

        private readonly CompositeDisposable _disposables = new();
        private readonly List<GameObject> _logEntryObjects = new();

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Init()
        {
            if (_runLogStore == null)
            {
                Debug.LogWarning("RunLogStore is not injected.");
                return;
            }

            if (_logEntryPrefab == null)
            {
                Debug.LogWarning("Log entry prefab is not assigned.");
                return;
            }

            if (_contentParent == null)
            {
                Debug.LogWarning("Content parent is not assigned.");
                return;
            }

            // 初期表示をクリア
            Clear();

            // 最新ログを購読
            _runLogStore.LatestEntry
                .ObserveOnMainThread()
                .Where(entry => entry != null)
                .Subscribe(OnNewEntry)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            DestroyAllLogEntries();
        }

        /// <summary>
        /// 新しいログが来たときの処理です。
        /// </summary>
        private void OnNewEntry(RunLogViewEntry entry)
        {
            CreateLogEntryObject(entry);
            RemoveOldLogEntries();
            ScrollToBottom();
        }

        /// <summary>
        /// ログのGameObjectを生成します。
        /// </summary>
        private void CreateLogEntryObject(RunLogViewEntry entry)
        {
            var go = Instantiate(_logEntryPrefab, _contentParent);
            go.SetActive(true);

            // TextMeshProUGUIコンポーネントを取得してテキストと色を設定
            var textComponent = go.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = entry.Message;
                textComponent.color = GetColorForType(entry.Type);
            }

            _logEntryObjects.Add(go);
        }

        /// <summary>
        /// 古いログを削除します。
        /// </summary>
        private void RemoveOldLogEntries()
        {
            while (_logEntryObjects.Count > _maxLogEntries)
            {
                var oldEntry = _logEntryObjects[0];
                _logEntryObjects.RemoveAt(0);

                if (oldEntry != null)
                {
                    Destroy(oldEntry);
                }
            }
        }

        /// <summary>
        /// ログタイプに応じた色を返します。
        /// </summary>
        private Color GetColorForType(RunLogViewType type)
        {
            return type switch
            {
                RunLogViewType.Damage => _damageColor,
                RunLogViewType.Defeat => _defeatColor,
                RunLogViewType.Heal => _healColor,
                _ => _systemColor
            };
        }

        /// <summary>
        /// スクロールを一番下に移動します。
        /// </summary>
        private void ScrollToBottom()
        {
            if (_scrollRect == null)
            {
                return;
            }

            // 次フレームでスクロール（ContentSizeFitterの更新を待つ）
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// すべてのログを削除します。
        /// </summary>
        private void DestroyAllLogEntries()
        {
            foreach (var go in _logEntryObjects)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }
            _logEntryObjects.Clear();
        }

        /// <summary>
        /// 表示をクリアします。
        /// </summary>
        public void Clear()
        {
            DestroyAllLogEntries();
        }
    }
}




