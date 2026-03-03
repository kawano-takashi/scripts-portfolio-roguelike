using System;
using UnityEngine;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;

namespace Roguelike.Presentation.Gameplay.Map.Views.Actor
{
    public class Enemy2DView : MonoBehaviour
    {
        private const int GeneratedSpriteSize = 16;

        private static readonly Color MeleeSpriteColor = new Color(0.85f, 0.25f, 0.25f, 1f);
        private static readonly Color RangedSpriteColor = new Color(0.25f, 0.85f, 0.25f, 1f);
        private static readonly Color DisruptorSpriteColor = new Color(0.9f, 0.7f, 0.15f, 1f);
        private static readonly Color FallbackSpriteColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Serializable]
        private sealed class DirectionSpriteSet
        {
            public Sprite Up;
            public Sprite UpRight;
            public Sprite Right;
            public Sprite DownRight;
            public Sprite Down;
            public Sprite DownLeft;
            public Sprite Left;
            public Sprite UpLeft;

            public Sprite Get(DirectionDto direction, Sprite fallback)
            {
                var sprite = direction switch
                {
                    DirectionDto.Up => Up,
                    DirectionDto.UpRight => UpRight,
                    DirectionDto.Right => Right,
                    DirectionDto.DownRight => DownRight,
                    DirectionDto.Down => Down,
                    DirectionDto.DownLeft => DownLeft,
                    DirectionDto.Left => Left,
                    DirectionDto.UpLeft => UpLeft,
                    _ => Down
                };

                return sprite ?? FirstNonNull() ?? fallback;
            }

            private Sprite FirstNonNull()
            {
                return Up ?? UpRight ?? Right ?? DownRight ?? Down ?? DownLeft ?? Left ?? UpLeft;
            }
        }

        [Header("Sprites")]
        [SerializeField] private DirectionSpriteSet _meleeSprites = new DirectionSpriteSet();
        [SerializeField] private DirectionSpriteSet _rangedSprites = new DirectionSpriteSet();
        [SerializeField] private DirectionSpriteSet _disruptorSprites = new DirectionSpriteSet();
        [SerializeField] private Sprite _fallbackSprite;

        private SpriteRenderer _spriteRenderer;
        private bool _spritesInitialized;

        /// <summary>
        /// 攻撃演出から参照するためのSpriteRendererです。
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                EnsureSpriteRenderer();
                return _spriteRenderer;
            }
        }

        public void Init()
        {
            EnsureSpriteRenderer();
            InitializeDirectionSprites();
        }

        public void Apply(EnemySnapshotDto enemy, Vector2 offset)
        {
            if (enemy.ActorId == Guid.Empty)
            {
                return;
            }

            EnsureSpriteRenderer();
            InitializeDirectionSprites();
            transform.position = new Vector3(enemy.Position.X + offset.x, enemy.Position.Y + offset.y, 0f);
            _spriteRenderer.enabled = true;

            var sprite = GetSprite(enemy);
            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
        }

        private Sprite GetSprite(EnemySnapshotDto enemy)
        {
            var facing = enemy.Facing;
            var archetype = enemy.EnemyArchetype;

            return archetype switch
            {
                EnemyArchetypeDto.Melee => _meleeSprites.Get(facing, _fallbackSprite),
                EnemyArchetypeDto.Ranged => _rangedSprites.Get(facing, _fallbackSprite),
                EnemyArchetypeDto.Disruptor => _disruptorSprites.Get(facing, _fallbackSprite),
                _ => _meleeSprites.Get(facing, _fallbackSprite)
            };
        }

        private void InitializeDirectionSprites()
        {
            if (_spritesInitialized)
            {
                return;
            }

            EnsureDirectionSprites(_meleeSprites, MeleeSpriteColor);
            EnsureDirectionSprites(_rangedSprites, RangedSpriteColor);
            EnsureDirectionSprites(_disruptorSprites, DisruptorSpriteColor);

            if (_fallbackSprite == null)
            {
                _fallbackSprite = CreateDirectionSprite(DirectionDto.Down, FallbackSpriteColor);
            }

            _spritesInitialized = true;
        }

        private void EnsureDirectionSprites(DirectionSpriteSet sprites, Color color)
        {
            if (sprites == null)
            {
                return;
            }

            if (sprites.Up == null) sprites.Up = CreateDirectionSprite(DirectionDto.Up, color);
            if (sprites.UpRight == null) sprites.UpRight = CreateDirectionSprite(DirectionDto.UpRight, color);
            if (sprites.Right == null) sprites.Right = CreateDirectionSprite(DirectionDto.Right, color);
            if (sprites.DownRight == null) sprites.DownRight = CreateDirectionSprite(DirectionDto.DownRight, color);
            if (sprites.Down == null) sprites.Down = CreateDirectionSprite(DirectionDto.Down, color);
            if (sprites.DownLeft == null) sprites.DownLeft = CreateDirectionSprite(DirectionDto.DownLeft, color);
            if (sprites.Left == null) sprites.Left = CreateDirectionSprite(DirectionDto.Left, color);
            if (sprites.UpLeft == null) sprites.UpLeft = CreateDirectionSprite(DirectionDto.UpLeft, color);
        }

        private static Sprite CreateDirectionSprite(DirectionDto direction, Color color)
        {
            var texture = new Texture2D(GeneratedSpriteSize, GeneratedSpriteSize);
            var colors = new Color[GeneratedSpriteSize * GeneratedSpriteSize];

            var isDiagonal = direction == DirectionDto.UpRight
                || direction == DirectionDto.DownRight
                || direction == DirectionDto.DownLeft
                || direction == DirectionDto.UpLeft;
            var diagonalDir = Vector2.zero;
            var diagonalPerp = Vector2.zero;
            const float diagonalTail = -2.5f;
            const float diagonalTip = 7.0f;
            const float diagonalBaseWidth = 3.5f;
            var center = (GeneratedSpriteSize - 1) / 2f;
            var max = GeneratedSpriteSize - 1;
            if (isDiagonal)
            {
                diagonalDir = GetDirectionVector(direction).normalized;
                diagonalPerp = new Vector2(-diagonalDir.y, diagonalDir.x);
            }

            for (int y = 0; y < GeneratedSpriteSize; y++)
            {
                for (int x = 0; x < GeneratedSpriteSize; x++)
                {
                    colors[y * GeneratedSpriteSize + x] = new Color(0, 0, 0, 0);

                    if (isDiagonal)
                    {
                        var p = new Vector2(x - center, y - center);
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
                            colors[y * GeneratedSpriteSize + x] = color;
                        }

                        continue;
                    }

                    switch (direction)
                    {
                        case DirectionDto.Up:
                            if (x >= (max - y) / 2f && x <= (max + y) / 2f)
                                colors[y * GeneratedSpriteSize + x] = color;
                            break;
                        case DirectionDto.Down:
                            if (x >= y / 2f && x <= (2f * max - y) / 2f)
                                colors[y * GeneratedSpriteSize + x] = color;
                            break;
                        case DirectionDto.Right:
                            if (y <= -0.5f * x + max && y >= 0.5f * x)
                                colors[y * GeneratedSpriteSize + x] = color;
                            break;
                        case DirectionDto.Left:
                            if (y >= -0.5f * x + center && y <= 0.5f * x + center)
                                colors[y * GeneratedSpriteSize + x] = color;
                            break;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, GeneratedSpriteSize, GeneratedSpriteSize),
                new Vector2(0.5f, 0.5f),
                GeneratedSpriteSize);
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

        private void EnsureSpriteRenderer()
        {
            if (_spriteRenderer != null)
            {
                return;
            }

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

        }
    }
}



