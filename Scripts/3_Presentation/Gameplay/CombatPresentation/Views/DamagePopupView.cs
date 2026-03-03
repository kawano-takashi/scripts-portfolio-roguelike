using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;
using VContainer;
using DG.Tweening;
using Roguelike.Application.Dtos;
using Roguelike.Presentation.Gameplay.Map.Contracts;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Views
{
    /// <summary>
    /// ダメージポップアップを表示するビューです。
    /// アクターがダメージを受けたときに、頭上に数値を表示してフェードアウトします。
    /// </summary>
    public sealed class DamagePopupView : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 2f)]
        private float _floatDistance = 0.5f;

        [SerializeField, Range(0.1f, 2f)]
        private float _floatDuration = 0.6f;

        [SerializeField, Range(0f, 1f)]
        private float _fadeDelay = 0.2f;

        [SerializeField, Range(0.1f, 1f)]
        private float _fadeDuration = 0.4f;

        [Header("Text Settings")]
        [SerializeField, Range(4, 72)]
        private int _fontSize = 36;

        [SerializeField]
        private Color _damageColor = Color.white;

        [SerializeField]
        private Vector2 _spawnOffset = new Vector2(0f, 0f);

        [Header("Pool Settings")]
        [SerializeField, Range(1, 50)]
        private int _initialPoolSize = 4;

        [SerializeField, Range(1, 100)]
        private int _maxPoolSize = 10;

        [Inject] private IGameplayActorViewLocator _actorViewLocator;

        private ObjectPool<GameObject> _popupPool;
        private Transform _popupRoot;
        private const string SortingLayerName = "Player";
        private const int SortingOrder = 20;

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Init()
        {
            EnsurePopupRoot();
            InitializePool();
        }

        /// <summary>
        /// ダメージポップアップを表示します。
        /// </summary>
        /// <param name="targetId">ダメージを受けたアクターのID</param>
        /// <param name="fallbackPosition">ビューが見つからない場合の位置</param>
        /// <param name="amount">ダメージ量</param>
        public void ShowDamage(Guid targetId, GridPositionDto fallbackPosition, int amount)
        {
            if (_popupPool == null)
            {
                InitializePool();
            }

            var worldPosition = GetWorldPosition(targetId, fallbackPosition);
            var spawnPosition = worldPosition + new Vector3(_spawnOffset.x, _spawnOffset.y, 0f);

            var popup = _popupPool.Get();
            popup.transform.position = spawnPosition;

            var textComponent = popup.GetComponent<TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = amount.ToString();
                textComponent.color = _damageColor;
            }

            AnimatePopup(popup);
        }

        private void EnsurePopupRoot()
        {
            if (_popupRoot != null)
            {
                return;
            }

            var root = new GameObject("DamagePopups");
            root.transform.SetParent(transform, false);
            _popupRoot = root.transform;
        }

        private void InitializePool()
        {
            EnsurePopupRoot();

            _popupPool = new ObjectPool<GameObject>(
                createFunc: CreatePopupInstance,
                actionOnGet: OnGetPopup,
                actionOnRelease: OnReleasePopup,
                actionOnDestroy: OnDestroyPopup,
                collectionCheck: false,
                defaultCapacity: _initialPoolSize,
                maxSize: _maxPoolSize);

            // 初期プール数だけ事前に生成します。
            var prewarmed = new List<GameObject>(_initialPoolSize);
            for (var i = 0; i < _initialPoolSize; i++)
            {
                prewarmed.Add(_popupPool.Get());
            }

            foreach (var popup in prewarmed)
            {
                _popupPool.Release(popup);
            }
        }

        private GameObject CreatePopupInstance()
        {
            var popup = new GameObject("DamagePopup");
            popup.transform.SetParent(_popupRoot, false);

            var textComponent = popup.AddComponent<TextMeshPro>();
            textComponent.fontSize = _fontSize;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.color = _damageColor;
            textComponent.sortingLayerID = SortingLayer.NameToID(SortingLayerName);
            textComponent.sortingOrder = SortingOrder;

            // MeshRendererのソート設定もします。
            var meshRenderer = popup.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = SortingLayerName;
                meshRenderer.sortingOrder = SortingOrder;
            }

            popup.SetActive(false);
            return popup;
        }

        private static void OnGetPopup(GameObject popup)
        {
            popup.SetActive(true);
        }

        private static void OnReleasePopup(GameObject popup)
        {
            popup.SetActive(false);
            popup.transform.DOKill();
        }

        private static void OnDestroyPopup(GameObject popup)
        {
            if (popup != null)
            {
                popup.transform.DOKill();
                Destroy(popup);
            }
        }

        private void AnimatePopup(GameObject popup)
        {
            var startPosition = popup.transform.position;
            var endPosition = startPosition + new Vector3(0f, _floatDistance, 0f);

            var textComponent = popup.GetComponent<TextMeshPro>();

            var sequence = DOTween.Sequence();

            // 上昇アニメーション
            sequence.Append(
                popup.transform.DOMove(endPosition, _floatDuration)
                    .SetEase(Ease.OutQuad));

            // フェードアウト（遅延開始）
            if (textComponent != null)
            {
                sequence.Insert(
                    _fadeDelay,
                    textComponent.DOFade(0f, _fadeDuration)
                        .SetEase(Ease.InQuad));
            }

            // アニメーション完了後にプールに返却
            sequence.OnComplete(() => ReturnToPool(popup, textComponent));
        }

        private void ReturnToPool(GameObject popup, TextMeshPro textComponent)
        {
            if (popup == null)
            {
                return;
            }

            // 透明度をリセットします。
            if (textComponent != null)
            {
                var color = textComponent.color;
                color.a = 1f;
                textComponent.color = color;
            }

            if (_popupPool != null)
            {
                _popupPool.Release(popup);
            }
            else
            {
                Destroy(popup);
            }
        }

        private Vector3 GetWorldPosition(Guid actorId, GridPositionDto fallbackPosition)
        {
            if (_actorViewLocator != null &&
                _actorViewLocator.TryResolve(actorId, out var actorTransform, out _) &&
                actorTransform != null)
            {
                return actorTransform.position;
            }

            // ビューが見つからない場合はマス座標をそのまま使います。
            return new Vector3(fallbackPosition.X, fallbackPosition.Y, 0f);
        }

        private void OnDestroy()
        {
            _popupPool?.Dispose();
        }
    }
}




