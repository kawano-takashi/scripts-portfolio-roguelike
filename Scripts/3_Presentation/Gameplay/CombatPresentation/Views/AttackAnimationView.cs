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
    /// 攻撃アニメーションを再生するビューです。
    /// </summary>
    public sealed class AttackAnimationView : MonoBehaviour
    {
        [Header("Melee")]
        [SerializeField] private float _meleeLungeDistance = 0.2f;
        [SerializeField] private float _meleeLungeDuration = 0.08f;
        [SerializeField] private float _meleeReturnDuration = 0.1f;
        [SerializeField] private Ease _meleeLungeEase = Ease.OutQuad;
        [SerializeField] private Ease _meleeReturnEase = Ease.InQuad;

        [Header("Ranged")]
        [SerializeField] private float _projectileDuration = 0.18f;
        [SerializeField] private Ease _projectileEase = Ease.Linear;
        [SerializeField] private Color _projectileColor = new Color(1f, 0.8f, 0.3f, 1f);
        [SerializeField] private float _projectileScale = 0.15f;
        [SerializeField, Range(2, 16)] private int _projectileSpriteSize = 4;

        [Header("Hit Blink")]
        [SerializeField, Range(1, 6)] private int _hitBlinkCount = 3;
        [SerializeField] private float _hitBlinkDuration = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _hitBlinkAlpha = 0.2f;

        [Header("General")]
        [SerializeField] private Vector2 _offset = new Vector2(0f, 0f);

        [Inject] private IGameplayActorViewLocator _actorViewLocator;

        private Sprite _projectileSprite;
        private Transform _effectRoot;
        private const string sortingLayerName = "Player";

        public void Init()
        {
            // 演出用のルートを確保し、投射物用の簡易スプライトを準備します。
            EnsureEffectRoot();
            _projectileSprite = CreateProjectileSprite(_projectileSpriteSize);
        }

        public async UniTask PlayAttackAsync(AttackAnimationRequest request, CancellationToken token)
        {
            EnsureEffectRoot();
            if (_projectileSprite == null)
            {
                _projectileSprite = CreateProjectileSprite(_projectileSpriteSize);
            }

            await PlayAttackAnimation(request, token);
        }

        private async UniTask PlayAttackAnimation(AttackAnimationRequest request, CancellationToken token)
        {
            // 実際のビュー位置を優先し、見つからなければグリッド座標を使います。
            var attackerWorld = GetWorldPosition(request.AttackerId, request.AttackerPosition, out var attackerTransform, out _);
            var targetWorld = GetWorldPosition(request.TargetId, request.TargetPosition, out _, out var targetRenderer);

            if (request.Kind == AttackKindDto.Melee)
            {
                await PlayMeleeLunge(attackerTransform, targetWorld, token);
            }
            else
            {
                await PlayRangedProjectile(attackerWorld, targetWorld, token);
            }

            if (request.ShowHitEffect)
            {
                await PlayHitBlink(targetRenderer, token);
            }
        }

        private async UniTask PlayMeleeLunge(Transform attackerTransform, Vector3 targetWorld, CancellationToken token)
        {
            if (attackerTransform == null)
            {
                return;
            }

            var origin = attackerTransform.position;
            var direction = targetWorld - (origin + new Vector3(_offset.x, _offset.y, 0f));
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var offset = direction.normalized * _meleeLungeDistance;
            var sequence = DOTween.Sequence();
            // 踏み込み → 元に戻る、の短い往復です。
            sequence.Append(attackerTransform.DOMove(origin + offset, _meleeLungeDuration).SetEase(_meleeLungeEase));
            sequence.Append(attackerTransform.DOMove(origin, _meleeReturnDuration).SetEase(_meleeReturnEase));

            var duration = sequence.Duration();
            if (duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }
        }

        private async UniTask PlayRangedProjectile(Vector3 startWorld, Vector3 targetWorld, CancellationToken token)
        {
            // シンプルな投射物を生成して直線移動させます。
            var projectile = CreateProjectileInstance(startWorld);
            if (projectile == null)
            {
                return;
            }

            var tween = projectile.transform.DOMove(targetWorld, _projectileDuration).SetEase(_projectileEase);
            var duration = tween.Duration();

            if (duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }

            if (projectile != null)
            {
                // 到達後に破棄します。
                Destroy(projectile);
            }
        }

        private async UniTask PlayHitBlink(SpriteRenderer targetRenderer, CancellationToken token)
        {
            if (targetRenderer == null)
            {
                return;
            }

            var originalColor = targetRenderer.color;
            targetRenderer.DOKill();

            // 透明度を揺らして被弾点滅を表現します。
            var tween = targetRenderer.DOFade(_hitBlinkAlpha, _hitBlinkDuration)
                .SetLoops(_hitBlinkCount * 2, LoopType.Yoyo)
                .SetEase(Ease.Linear);

            var duration = tween.Duration();
            if (duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }

            targetRenderer.color = originalColor;
        }

        private Vector3 GetWorldPosition(Guid? actorId, GridPositionDto fallbackPosition, out Transform actorTransform, out SpriteRenderer spriteRenderer)
        {
            actorTransform = null;
            spriteRenderer = null;

            if (!actorId.HasValue)
            {
                return new Vector3(fallbackPosition.X, fallbackPosition.Y, 0f);
            }

            var resolvedId = actorId.Value;

            // プレイヤーかどうかを最初に判定します。
            if (_actorViewLocator != null &&
                _actorViewLocator.TryResolve(resolvedId, out actorTransform, out spriteRenderer) &&
                actorTransform != null)
            {
                return actorTransform.position + new Vector3(_offset.x, _offset.y, 0f);
            }

            // ビューが見つからない場合はマス座標をそのまま使います。
            return new Vector3(fallbackPosition.X, fallbackPosition.Y, 0f);
        }

        private void EnsureEffectRoot()
        {
            if (_effectRoot != null)
            {
                return;
            }

            // 演出用のGameObjectをまとめる親を作成します。
            var root = new GameObject("AttackEffects");
            root.transform.SetParent(transform, false);
            _effectRoot = root.transform;
        }

        private GameObject CreateProjectileInstance(Vector3 position)
        {
            EnsureEffectRoot();

            // 使い捨ての投射物を作成します（Prefabは使わない）。
            var projectile = new GameObject("AttackProjectile");
            projectile.transform.SetParent(_effectRoot, false);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * _projectileScale;

            var renderer = projectile.AddComponent<SpriteRenderer>();
            renderer.sprite = _projectileSprite;
            renderer.color = _projectileColor;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = 10;

            return projectile;
        }

        private static Sprite CreateProjectileSprite(int size)
        {
            // 四角い簡易スプライトを動的に生成します。
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



