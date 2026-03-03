using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Map.Views.Map
{
    /// <summary>
    /// ミニマップ描画の受動ビューです。
    /// </summary>
    public sealed class MiniMapView : MonoBehaviour, IMiniMapView
    {
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _objectsTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase _floorTile;
        [SerializeField] private TileBase _entranceTile;
        [SerializeField] private TileBase _stairsDownTile;
        [SerializeField] private TileBase _treasureTile;
        [SerializeField] private TileBase _bossTile;
        [SerializeField] private TileBase _playerTile;
        [SerializeField] private TileBase _enemyMeleeTile;
        [SerializeField] private TileBase _enemyRangedTile;
        [SerializeField] private TileBase _enemyDisruptorTile;
        [FormerlySerializedAs("_itemFuelTile")]
        [SerializeField] private TileBase _itemFoodTile;
        [SerializeField] private TileBase _itemHealingTile;
        [SerializeField] private TileBase _itemSpellbookTile;
        [SerializeField] private TileBase _itemArmorTile;

        [Header("Minimap Camera")]
        [SerializeField] private Camera _minimapCamera;
        [SerializeField] private float _cameraPadding = 1f;
        [SerializeField] private Color _exploredTint = new Color(0.35f, 0.35f, 0.35f, 1f);

        [Header("Render Texture")]
        [SerializeField] private RawImage _minimapTarget;
        [SerializeField] private Vector2Int _renderTextureSize = new Vector2Int(256, 256);
        [SerializeField] private FilterMode _renderTextureFilterMode = FilterMode.Point;
        [SerializeField] private int _renderTextureDepth = 16;

        private RenderTexture _renderTexture;
        private int _lastMapWidth = -1;
        private int _lastMapHeight = -1;

        public void Init()
        {
            SetupRenderTexture();
        }

        private void OnDestroy()
        {
            ReleaseRenderTexture();
        }

        public void Render(MiniMapDisplayModel model)
        {
            if (model == null)
            {
                return;
            }

            var mapChanged = _lastMapWidth != model.Map.Width || _lastMapHeight != model.Map.Height;
            _lastMapWidth = model.Map.Width;
            _lastMapHeight = model.Map.Height;
            if (mapChanged)
            {
                FitCameraToMap(model.Map);
            }

            RenderMiniMap(model);
        }

        private void RenderMiniMap(MiniMapDisplayModel model)
        {
            if (_floorTilemap == null || _objectsTilemap == null)
            {
                return;
            }

            _floorTilemap.ClearAllTiles();
            _objectsTilemap.ClearAllTiles();

            var map = model.Map;
            var visibility = new Dictionary<(int x, int y), bool>(map.Tiles.Count);

            for (var i = 0; i < map.Tiles.Count; i++)
            {
                var tile = map.Tiles[i];
                visibility[(tile.Position.X, tile.Position.Y)] = tile.IsVisible;
                if (!tile.IsExplored)
                {
                    continue;
                }

                var tilePosition = new Vector3Int(tile.Position.X, tile.Position.Y, 0);
                switch (tile.TileType)
                {
                    case TileTypeDto.Floor:
                    case TileTypeDto.DoorOpen:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _floorTilemap.SetColor(tilePosition, _exploredTint);
                        break;
                    case TileTypeDto.StairsDown:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _objectsTilemap.SetTile(tilePosition, _stairsDownTile);
                        _floorTilemap.SetColor(tilePosition, _exploredTint);
                        _objectsTilemap.SetColor(tilePosition, _exploredTint);
                        break;
                }
            }

            var occupied = new HashSet<(int x, int y)>();
            for (var i = 0; i < model.Enemies.Count; i++)
            {
                var enemy = model.Enemies[i];
                var key = (enemy.Position.X, enemy.Position.Y);
                if (!visibility.TryGetValue(key, out var isVisible) || !isVisible)
                {
                    continue;
                }

                var tile = GetEnemyTile(enemy);
                if (tile == null)
                {
                    continue;
                }

                var tilePosition = new Vector3Int(enemy.Position.X, enemy.Position.Y, 0);
                _objectsTilemap.SetTile(tilePosition, tile);
                _objectsTilemap.SetColor(tilePosition, _exploredTint);
                occupied.Add(key);
            }

            for (var i = 0; i < model.GroundItems.Count; i++)
            {
                var item = model.GroundItems[i];
                var key = (item.Position.X, item.Position.Y);
                if (occupied.Contains(key))
                {
                    continue;
                }

                if (!visibility.TryGetValue(key, out var isVisible) || !isVisible)
                {
                    continue;
                }

                var tile = GetItemTile(item.ItemType);
                if (tile == null)
                {
                    continue;
                }

                var tilePosition = new Vector3Int(item.Position.X, item.Position.Y, 0);
                _objectsTilemap.SetTile(tilePosition, tile);
                _objectsTilemap.SetColor(tilePosition, _exploredTint);
            }

            var playerKey = (model.Player.Position.X, model.Player.Position.Y);
            if (_playerTile != null &&
                visibility.TryGetValue(playerKey, out var playerVisible) &&
                playerVisible)
            {
                var playerPosition = new Vector3Int(model.Player.Position.X, model.Player.Position.Y, 0);
                _objectsTilemap.SetTile(playerPosition, _playerTile);
                _objectsTilemap.SetColor(playerPosition, _exploredTint);
            }
        }

        private void FitCameraToMap(MapSnapshotDto map)
        {
            if (_minimapCamera == null || _floorTilemap == null)
            {
                return;
            }

            var width = map.Width;
            var height = map.Height;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var cellSize = _floorTilemap.cellSize;
            var scale = _floorTilemap.transform.lossyScale;

            var widthWorld = width * cellSize.x * scale.x;
            var heightWorld = height * cellSize.y * scale.y;
            var centerLocal = new Vector3((width - 1) * cellSize.x * 0.5f, (height - 1) * cellSize.y * 0.5f, 0f);
            var centerWorld = _floorTilemap.transform.TransformPoint(centerLocal);

            var camTransform = _minimapCamera.transform;
            camTransform.position = new Vector3(centerWorld.x, centerWorld.y, camTransform.position.z);

            if (!_minimapCamera.orthographic)
            {
                return;
            }

            var aspect = GetRenderAspect();
            var halfHeight = heightWorld * 0.5f;
            var halfWidth = widthWorld * 0.5f;
            var size = Math.Max(halfHeight, halfWidth / aspect);
            _minimapCamera.orthographicSize = size + _cameraPadding;
        }

        private void SetupRenderTexture()
        {
            if (_minimapCamera == null || _minimapTarget == null)
            {
                return;
            }

            var size = ResolveRenderTextureSize();
            if (_renderTexture != null && _renderTexture.width == size.x && _renderTexture.height == size.y)
            {
                _minimapCamera.targetTexture = _renderTexture;
                _minimapTarget.texture = _renderTexture;
                return;
            }

            ReleaseRenderTexture();

            _renderTexture = new RenderTexture(size.x, size.y, _renderTextureDepth, RenderTextureFormat.ARGB32)
            {
                name = "MiniMapRenderTexture",
                filterMode = _renderTextureFilterMode,
                useMipMap = false,
                autoGenerateMips = false
            };
            _renderTexture.Create();

            _minimapCamera.targetTexture = _renderTexture;
            _minimapTarget.texture = _renderTexture;
        }

        private Vector2Int ResolveRenderTextureSize()
        {
            if (_renderTextureSize.x > 0 && _renderTextureSize.y > 0)
            {
                return _renderTextureSize;
            }

            if (_minimapTarget != null)
            {
                var rect = _minimapTarget.rectTransform.rect;
                var width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
                var height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
                if (width > 0 && height > 0)
                {
                    return new Vector2Int(width, height);
                }
            }

            return new Vector2Int(256, 256);
        }

        private float GetRenderAspect()
        {
            if (_minimapCamera != null && _minimapCamera.targetTexture != null)
            {
                return (float)_minimapCamera.targetTexture.width / _minimapCamera.targetTexture.height;
            }

            return _minimapCamera != null && _minimapCamera.aspect > 0f ? _minimapCamera.aspect : 1f;
        }

        private void ReleaseRenderTexture()
        {
            if (_minimapCamera != null && _minimapCamera.targetTexture == _renderTexture)
            {
                _minimapCamera.targetTexture = null;
            }

            if (_minimapTarget != null && _minimapTarget.texture == _renderTexture)
            {
                _minimapTarget.texture = null;
            }

            if (_renderTexture == null)
            {
                return;
            }

            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

        private TileBase GetEnemyTile(EnemySnapshotDto enemy)
        {
            return enemy.EnemyArchetype switch
            {
                EnemyArchetypeDto.Melee => _enemyMeleeTile,
                EnemyArchetypeDto.Ranged => _enemyRangedTile,
                EnemyArchetypeDto.Disruptor => _enemyDisruptorTile,
                _ => _enemyMeleeTile
            };
        }

        private TileBase GetItemTile(ItemTypeDto itemType)
        {
            if (itemType.IsSpellbook())
            {
                return _itemSpellbookTile;
            }

            return itemType switch
            {
                ItemTypeDto.FoodRation => _itemFoodTile,
                ItemTypeDto.HealingPotion => _itemHealingTile,
                ItemTypeDto.Armor => _itemArmorTile,
                _ => null
            };
        }
    }
}




