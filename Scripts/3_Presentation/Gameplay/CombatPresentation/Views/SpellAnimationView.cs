using System;
using System.Threading;
using UnityEngine;
using VContainer;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.Map.Contracts;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Views
{
    /// <summary>
    /// スペル詠唱アニメーションを再生するビューです。
    /// </summary>
    public sealed class SpellAnimationView : MonoBehaviour
    {
        [Header("ForceBolt")]
        [SerializeField] private float _forceBoltDuration = 0.25f;
        [SerializeField] private Color _forceBoltColor = new Color(0.3f, 0.8f, 1f, 1f);
        [SerializeField] private float _forceBoltScale = 0.2f;

        [Header("Sleep")]
        [SerializeField] private float _sleepDuration = 0.3f;
        [SerializeField] private Color _sleepColor = new Color(0.6f, 0.4f, 0.8f, 1f);
        [SerializeField] private float _sleepScale = 0.15f;

        [Header("MagicFire")]
        [SerializeField] private float _magicFireDuration = 0.25f;
        [SerializeField] private Color _magicFireColor = new Color(1f, 0.45f, 0.2f, 1f);
        [SerializeField] private float _magicFireScale = 0.2f;

        [Header("General")]
        [SerializeField] private Ease _projectileEase = Ease.Linear;
        [SerializeField, Range(2, 16)] private int _spriteSize = 4;
        [SerializeField] private float _waitDuration = 0.3f;
        [SerializeField] private Vector2 _positionOffset = Vector2.zero;
        [SerializeField] private Vector2 _spawnOffset = Vector2.zero;

        [Inject] private IGameplayActorViewLocator _actorViewLocator;

        private Sprite _projectileSprite;
        private Transform _effectRoot;
        private const string SortingLayerName = "Player";

        public void Init()
        {
            EnsureEffectRoot();
            _projectileSprite = CreateProjectileSprite(_spriteSize);
        }

        /// <summary>
        /// スペル詠唱アニメーションを再生します。
        /// </summary>
        public async UniTask PlaySpellAsync(SpellAnimationRequest request, CancellationToken token)
        {
            EnsureEffectRoot();
            if (_projectileSprite == null)
            {
                _projectileSprite = CreateProjectileSprite(_spriteSize);
            }

            // スペル種別に応じたアニメーションパラメータを選択
            var (color, scale, duration) = GetSpellParameters(request.Spell);

            var startWorld = GetWorldPosition(request.CasterId, request.CasterPosition);
            var targetWorld = GetWorldPosition(request.TargetId, request.TargetPosition);
            // 仕様:
            // - 装備詠唱 (true)  は即発射したいので待機しない
            // - 非装備詠唱 (false) は従来どおり _waitDuration だけ待機する
            var shouldWaitBeforeProjectile = !request.IsEquippedSpellCast;

            await PlayProjectile(startWorld, targetWorld, color, scale, duration, shouldWaitBeforeProjectile, token);
        }

        private (Color color, float scale, float duration) GetSpellParameters(ItemTypeDto spell)
        {
            return spell switch
            {
                ItemTypeDto.SpellbookForceBolt => (_forceBoltColor, _forceBoltScale, _forceBoltDuration),
                ItemTypeDto.SpellbookMagicFire => (_magicFireColor, _magicFireScale, _magicFireDuration),
                ItemTypeDto.SpellbookSleep => (_sleepColor, _sleepScale, _sleepDuration),
                _ => (_forceBoltColor, _forceBoltScale, _forceBoltDuration)
            };
        }

        private async UniTask PlayProjectile(
            Vector3 startWorld,
            Vector3 targetWorld,
            Color color,
            float scale,
            float duration,
            bool shouldWaitBeforeProjectile,
            CancellationToken token)
        {
            var projectile = CreateProjectileInstance(startWorld, color, scale);
            if (projectile == null)
            {
                return;
            }

            var sequence = DOTween.Sequence();
            // 発射前ウェイトは「非装備詠唱」のときだけ入れます。
            if (shouldWaitBeforeProjectile && _waitDuration > 0f)
            {
                sequence.AppendInterval(_waitDuration);
            }

            sequence.Append(projectile.transform.DOMove(targetWorld, duration).SetEase(_projectileEase));
            var tweenDuration = sequence.Duration();

            if (tweenDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(tweenDuration), cancellationToken: token);
            }

            if (projectile != null)
            {
                Destroy(projectile);
            }
        }

        private Vector3 GetWorldPosition(Guid? actorId, GridPositionDto position)
        {
            if (!actorId.HasValue)
            {
                // ターゲットがいない場合はマス座標にオフセットを適用します。
                return new Vector3(position.X + _positionOffset.x + _spawnOffset.x, position.Y + _positionOffset.y + _spawnOffset.y, 0f);
            }

            return GetWorldPosition(actorId.Value, position);
        }

        private Vector3 GetWorldPosition(Guid actorId, GridPositionDto fallbackPosition)
        {
            if (_actorViewLocator != null &&
                _actorViewLocator.TryResolve(actorId, out var actorTransform, out _) &&
                actorTransform != null)
            {
                return actorTransform.position + new Vector3(_spawnOffset.x, _spawnOffset.y, 0f);
            }

            // ビューが見つからない場合はマス座標にオフセットを適用します。
            return new Vector3(fallbackPosition.X + _positionOffset.x + _spawnOffset.x, fallbackPosition.Y + _positionOffset.y + _spawnOffset.y, 0f);
        }

        private void EnsureEffectRoot()
        {
            if (_effectRoot != null)
            {
                return;
            }

            var root = new GameObject("SpellEffects");
            root.transform.SetParent(transform, false);
            _effectRoot = root.transform;
        }

        private GameObject CreateProjectileInstance(Vector3 position, Color color, float scale)
        {
            EnsureEffectRoot();

            var projectile = new GameObject("SpellProjectile");
            projectile.transform.SetParent(_effectRoot, false);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * scale;

            var renderer = projectile.AddComponent<SpriteRenderer>();
            renderer.sprite = _projectileSprite;
            renderer.color = color;
            renderer.sortingLayerName = SortingLayerName;
            renderer.sortingOrder = 10;

            return projectile;
        }

        private static Sprite CreateProjectileSprite(int size)
        {
            var clampedSize = Mathf.Max(2, size);
            var texture = new Texture2D(clampedSize, clampedSize);
            var colors = new Color[clampedSize * clampedSize];

            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            texture.SetPixels(colors);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, clampedSize, clampedSize),
                new Vector2(0.5f, 0.5f),
                clampedSize);
        }
    }
}



