using System;
using UnityEngine;
using R3;
using VContainer;
using DG.Tweening;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.RunResult.Types;
using Roguelike.Presentation.Gameplay.FloorTransition.Presenters;
using Roguelike.Presentation.Gameplay.Guide.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;

namespace Roguelike.Presentation.Gameplay.Map.Views.Actor
{
    /// <summary>
    /// プレイヤーの見た目を出す係です。
    /// 位置と向きに合わせてスプライトを変えます。
    /// </summary>
    public class Player2DView : MonoBehaviour
    {
        // Roguelikeのターン通知から、位置と向きを受け取ります。
        [Inject] private readonly RunTurnStateStore _runTurnStateStore;

        [SerializeField] private Sprite _upSprite; // 上向きの絵
        [SerializeField] private Sprite _upRightSprite; // 右上向きの絵
        [SerializeField] private Sprite _rightSprite; // 右向きの絵
        [SerializeField] private Sprite _downRightSprite; // 右下向きの絵
        [SerializeField] private Sprite _downSprite; // 下向きの絵
        [SerializeField] private Sprite _downLeftSprite; // 左下向きの絵
        [SerializeField] private Sprite _leftSprite; // 左向きの絵
        [SerializeField] private Sprite _upLeftSprite; // 左上向きの絵
        [SerializeField] private Vector2 offset = new Vector2(0, 0); // マスの中心に合わせるズラし
        [SerializeField] private Camera _followCamera; // 追いかけるカメラ
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 0f, -10f); // プレイヤーからのズラし（2DはZを手前にする）

        [Header("Animation Settings")]
        [SerializeField] private Ease _moveEase = Ease.Linear; // 移動の動き方
        [SerializeField, Range(0.001f, 0.05f)] private float _dashMoveDuration = 0.02f;

        private readonly CompositeDisposable _disposables = new();
        private SpriteRenderer _spriteRenderer;

        // Animator関連（BlendTree用）
        private Animator _animator;
        private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
        private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

        // 移動アニメーション状態
        private bool _isMoving = false;
        private Sequence _currentMoveSequence;
        private Vector2 _previousPosition;
        private IDisposable _movementAnimationLock;

        /// <summary>
        /// 攻撃演出などからプレイヤーのスプライトを参照するためのアクセサです。
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }

                return _spriteRenderer;
            }
        }

        /// <summary>
        /// 準備をして、表示を始めます。
        /// </summary>
        public void Init()
        {
            Debug.Log("PlayerView initialized");

            // 追いかけるカメラが指定されていないなら、メインカメラを使います。
            if (_followCamera == null)
            {
                _followCamera = Camera.main;
            }

            // スプライトを描く部品を探します。
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // Animatorを探します。
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogWarning("Player2DView: Animator component not found. Falling back to static sprites.");
            }

            // 初期位置を保存します。
            var initialPos = _runTurnStateStore.PlayerPosition.CurrentValue;
            _previousPosition = new Vector2(initialPos.X, initialPos.Y);
            transform.position = new Vector3(_previousPosition.x + offset.x, _previousPosition.y + offset.y, 0);

            // 位置が変わったら、アニメーションで動かします。
            _runTurnStateStore.PlayerPosition
                .Subscribe(position =>
                {
                    var moveStyle = _runTurnStateStore.CurrentPlayerMoveStyle.CurrentValue;
                    AnimateMovement(new Vector2(position.X, position.Y), moveStyle);
                })
                .AddTo(_disposables);

            // 向きが変わったら、絵を差し替えます。
            _runTurnStateStore.PlayerFacingValue
                .Subscribe(value => UpdateFacingWithAnimation((DirectionDto)value))
                .AddTo(_disposables);

            // 最初にカメラの位置をそろえます。
            UpdateCameraPosition();
        }

        /// <summary>
        /// 画面の最後にカメラを動かして、プレイヤーを追いかけます。
        /// </summary>
        private void LateUpdate()
        {
            UpdateCameraPosition();
        }

        /// <summary>
        /// プレイヤーの移動をアニメーションで表現します。
        /// DOTweenで滑らかに動かし、歩行アニメーションを再生します。
        /// </summary>
        /// <param name="newPosition">新しい位置（マスの座標）</param>
        private void AnimateMovement(Vector2 newPosition, PlayerMoveStyle moveStyle)
        {
            // 位置が変わっていなければ何もしません。
            if (Vector2.Distance(_previousPosition, newPosition) < 0.01f)
            {
                return;
            }

            // いま動いているなら、先に終わらせます。
            if (_isMoving)
            {
                _currentMoveSequence?.Complete();
            }

            _isMoving = true;
            // 移動中は入力を受け付けないようにロックを取ります。
            _movementAnimationLock?.Dispose();
            _movementAnimationLock = _runTurnStateStore?.AcquireAnimationLock();

            // 歩行アニメーションを始めます。
            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, true);
            }

            // 目標位置を計算します。
            Vector3 targetPosition = new Vector3(newPosition.x + offset.x, newPosition.y + offset.y, 0);

            // アニメーションの組み合わせを作ります。
            var moveDuration = moveStyle == PlayerMoveStyle.Dash ? _dashMoveDuration : Constants.MOVE_DURATION;
            var clampedDuration = Mathf.Max(0.001f, moveDuration);
            _currentMoveSequence = DOTween.Sequence();
            _currentMoveSequence.Append(
                transform.DOMove(targetPosition, clampedDuration)
                    .SetEase(_moveEase)
            );

            // 動きが終わったらフラグを戻します。
            _currentMoveSequence.OnComplete(() =>
            {
                _isMoving = false;
                _previousPosition = newPosition;
                _movementAnimationLock?.Dispose();
                _movementAnimationLock = null;

                // 歩行アニメーションを止めます。
                if (_animator != null)
                {
                    _animator.SetBool(IsWalkingHash, false);
                }
            });
        }

        /// <summary>
        /// プレイヤーの位置に合わせてカメラを動かします。
        /// </summary>
        private void UpdateCameraPosition()
        {
            if (_followCamera == null)
            {
                return;
            }

            // プレイヤーの位置に、ズラしを足した場所へカメラを動かします。
            _followCamera.transform.position = transform.position + _cameraOffset;
        }

        /// <summary>
        /// プレイヤーの向きを画面に反映します。
        /// AnimatorがあればBlendTree用のX,Yパラメータを更新し、なければスプライトを切り替えます。
        /// </summary>
        /// <param name="facing">新しい向き</param>
        private void UpdateFacingWithAnimation(DirectionDto facing)
        {
            // 回転は使わず、絵を切り替えます。
            transform.rotation = Quaternion.identity;

            if (_animator != null)
            {
                // BlendTree用にX,Y座標を設定します。
                var blendDirection = GetBlendTreeDirection(facing);
                _animator.SetFloat(DirectionXHash, blendDirection.x);
                _animator.SetFloat(DirectionYHash, blendDirection.y);
                Debug.Log($"プレイヤー向き変更: {facing} → BlendTree ({blendDirection.x}, {blendDirection.y})");
            }
        }

        /// <summary>
        /// Direction enumをBlendTree用の2D座標に変換します。
        /// </summary>
        private static Vector2 GetBlendTreeDirection(DirectionDto direction)
        {
            return direction switch
            {
                DirectionDto.Up => new Vector2(0, -1),
                DirectionDto.UpRight => new Vector2(1, -1),
                DirectionDto.Right => new Vector2(1, 0),
                DirectionDto.DownRight => new Vector2(1, 1),
                DirectionDto.Down => new Vector2(0, 1),
                DirectionDto.DownLeft => new Vector2(-1, 1),
                DirectionDto.Left => new Vector2(-1, 0),
                DirectionDto.UpLeft => new Vector2(-1, -1),
                _ => Vector2.zero
            };
        }

        /// <summary>
        /// 向きに合わせた小さな矢印を作ります。
        /// </summary>
        /// <param name="direction">向き</param>
        /// <returns>作ったスプライト</returns>
        private Sprite CreateDirectionSprite(DirectionDto direction)
        {
            // 16x16の小さな絵を作ります。
            Texture2D texture = new Texture2D(16, 16);
            Color[] colors = new Color[16 * 16];

            // 三角形の形になるように色を付けます。
            var isDiagonal = direction == DirectionDto.UpRight
                || direction == DirectionDto.DownRight
                || direction == DirectionDto.DownLeft
                || direction == DirectionDto.UpLeft;
            var diagonalDir = Vector2.zero;
            var diagonalPerp = Vector2.zero;
            const float diagonalTail = -2.5f;
            const float diagonalTip = 7.0f;
            const float diagonalBaseWidth = 3.5f;
            if (isDiagonal)
            {
                diagonalDir = GetDirectionVector(direction).normalized;
                diagonalPerp = new Vector2(-diagonalDir.y, diagonalDir.x);
            }

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // 最初は透明にします。
                    colors[y * 16 + x] = new Color(0, 0, 0, 0);

                    if (isDiagonal)
                    {
                        var p = new Vector2(x - 7.5f, y - 7.5f);
                        var along = Vector2.Dot(p, diagonalDir);
                        if (along < diagonalTail || along > diagonalTip)
                        {
                            continue;
                        }

                        var t = (along - diagonalTail) / (diagonalTip - diagonalTail);
                        var halfWidth = Mathf.Lerp(diagonalBaseWidth, 0f, t);
                        var across = Mathf.Abs(Vector2.Dot(p, diagonalPerp));
                        if (across <= halfWidth)
                        {
                            colors[y * 16 + x] = Color.blue;
                        }

                        continue;
                    }

                    switch (direction)
                    {
                        case DirectionDto.Up:
                            // 上向きは、下が細くて上が広い三角形にします。
                            if (x >= (15 - y) / 2.0 && x <= (15 + y) / 2.0)
                                colors[y * 16 + x] = Color.blue;
                            break;
                        case DirectionDto.Down:
                            // 下向きは、上が細くて下が広い三角形にします。
                            if (x >= y / 2.0 && x <= (30 - y) / 2.0)
                                colors[y * 16 + x] = Color.blue;
                            break;
                        case DirectionDto.Right:
                            // 右向きは、右が細くて左が広い三角形にします。
                            if (y <= -0.5 * x + 15 && y >= 0.5 * x)
                                colors[y * 16 + x] = Color.blue;
                            break;
                        case DirectionDto.Left:
                            // 左向きは、左が細くて右が広い三角形にします。
                            if (y >= -0.5 * x + 7.5 && y <= 0.5 * x + 7.5)
                                colors[y * 16 + x] = Color.blue;
                            break;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            Debug.Log($"{direction}向きの矢印スプライトを作成しました");

            // 絵の中心を真ん中にします。
            return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        }

        private static Vector2 GetDirectionVector(DirectionDto direction)
        {
            return direction switch
            {
                DirectionDto.Up => new Vector2(0, -1),
                DirectionDto.UpRight => new Vector2(1, -1),
                DirectionDto.Right => new Vector2(1, 0),
                DirectionDto.DownRight => new Vector2(1, 1),
                DirectionDto.Down => new Vector2(0, 1),
                DirectionDto.DownLeft => new Vector2(-1, 1),
                DirectionDto.Left => new Vector2(-1, 0),
                DirectionDto.UpLeft => new Vector2(-1, -1),
                _ => Vector2.zero
            };
        }

        /// <summary>
        /// 片づけをします。
        /// </summary>
        private void OnDestroy()
        {
            // 動いているアニメを止めます。
            _currentMoveSequence?.Kill();
            _disposables.Dispose();
        }
    }
}




