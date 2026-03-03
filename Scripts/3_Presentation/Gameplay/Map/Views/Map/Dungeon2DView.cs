using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
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
    /// ダンジョンマップ描画の受動ビューです。
    /// </summary>
    public sealed class Dungeon2DView : MonoBehaviour, IDungeonMapView
    {
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private Tilemap _objectsTilemap;
        [SerializeField] private Tilemap _spellPreviewTilemap;

        [SerializeField] private TileBase _floorTile;
        [SerializeField] private TileBase _wallAutoTile;
        [SerializeField] private TileBase _entranceTile;
        [SerializeField] private TileBase _stairsDownTile;
        [SerializeField] private TileBase _treasureTile;
        [SerializeField] private TileBase _bossTile;

        [FormerlySerializedAs("_itemFuelTile")]
        [SerializeField] private TileBase _itemFoodTile;
        [SerializeField] private TileBase _itemHealingTile;
        [SerializeField] private TileBase _itemSpellbookTile;
        [SerializeField] private TileBase _itemArmorTile;
        [SerializeField] private TileBase _spellPreviewTile;

        private const int BorderSize = 8;
        [SerializeField] private Color _spellPreviewTint = new Color(1f, 0.35f, 0.35f, 0.35f);

        public void Init()
        {
            _floorTilemap?.ClearAllTiles();
            _wallTilemap?.ClearAllTiles();
            _objectsTilemap?.ClearAllTiles();
            _spellPreviewTilemap?.ClearAllTiles();
        }

        public void Render(DungeonMapDisplayModel model)
        {
            if (model == null)
            {
                return;
            }

            RenderRoguelikeMap(model.Map, model.GroundItems);
            RefreshSpellPreviewOverlay(model);
        }

        private void RenderRoguelikeMap(MapSnapshotDto map, IReadOnlyList<GroundItemSnapshotDto> groundItems)
        {
            if (_floorTilemap == null || _wallTilemap == null || _objectsTilemap == null)
            {
                return;
            }

            _floorTilemap.ClearAllTiles();
            _wallTilemap.ClearAllTiles();
            _objectsTilemap.ClearAllTiles();

            var groundItemByPosition = BuildGroundItemLookup(groundItems);

            for (var i = 0; i < map.Tiles.Count; i++)
            {
                var tile = map.Tiles[i];
                var position = tile.Position;
                var tilePosition = new Vector3Int(position.X, position.Y, 0);
                var tint = Color.white;

                switch (tile.TileType)
                {
                    case TileTypeDto.Wall:
                    case TileTypeDto.DoorClosed:
                    case TileTypeDto.Unknown:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _floorTilemap.SetColor(tilePosition, tint);
                        _wallTilemap.SetTile(tilePosition, _wallAutoTile);
                        _wallTilemap.SetColor(tilePosition, tint);
                        break;
                    case TileTypeDto.Floor:
                    case TileTypeDto.DoorOpen:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _floorTilemap.SetColor(tilePosition, tint);
                        break;
                    case TileTypeDto.StairsDown:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _objectsTilemap.SetTile(tilePosition, _stairsDownTile);
                        _floorTilemap.SetColor(tilePosition, tint);
                        _objectsTilemap.SetColor(tilePosition, tint);
                        break;
                    default:
                        _floorTilemap.SetTile(tilePosition, _floorTile);
                        _floorTilemap.SetColor(tilePosition, tint);
                        break;
                }

                if (!groundItemByPosition.TryGetValue((position.X, position.Y), out var groundItem))
                {
                    continue;
                }

                var itemTile = GetItemTile(groundItem.ItemType);
                if (itemTile == null)
                {
                    continue;
                }

                _objectsTilemap.SetTile(tilePosition, itemTile);
                _objectsTilemap.SetColor(tilePosition, tint);
            }

            RenderBorderWalls(map.Width, map.Height);
        }

        private static Dictionary<(int x, int y), GroundItemSnapshotDto> BuildGroundItemLookup(
            IReadOnlyList<GroundItemSnapshotDto> groundItems)
        {
            var lookup = new Dictionary<(int x, int y), GroundItemSnapshotDto>();
            if (groundItems == null)
            {
                return lookup;
            }

            for (var i = 0; i < groundItems.Count; i++)
            {
                var groundItem = groundItems[i];
                lookup[(groundItem.Position.X, groundItem.Position.Y)] = groundItem;
            }

            return lookup;
        }

        private void RenderBorderWalls(int mapWidth, int mapHeight)
        {
            var tint = Color.white;
            for (var x = -BorderSize; x < mapWidth + BorderSize; x++)
            {
                for (var y = -BorderSize; y < mapHeight + BorderSize; y++)
                {
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    {
                        continue;
                    }

                    var tilePosition = new Vector3Int(x, y, 0);
                    _wallTilemap.SetTile(tilePosition, _wallAutoTile);
                    _wallTilemap.SetColor(tilePosition, tint);
                }
            }
        }

        private void RefreshSpellPreviewOverlay(DungeonMapDisplayModel model)
        {
            if (_spellPreviewTilemap == null)
            {
                return;
            }

            _spellPreviewTilemap.ClearAllTiles();
            if (model == null || !model.IsSpellPreviewOpen)
            {
                return;
            }

            var previewPositions = model.SpellPreviewPositions;
            if (previewPositions == null || previewPositions.Count == 0)
            {
                return;
            }

            RenderSpellPreviewTiles(_spellPreviewTilemap, _spellPreviewTile, previewPositions, model.Map);
        }

        private void RenderSpellPreviewTiles(
            Tilemap previewTilemap,
            TileBase previewTile,
            IReadOnlyList<GridPositionDto> positions,
            MapSnapshotDto map)
        {
            if (previewTilemap == null || previewTile == null || positions == null)
            {
                return;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                if (position.X < 0 || position.X >= map.Width || position.Y < 0 || position.Y >= map.Height)
                {
                    continue;
                }

                var tilePosition = new Vector3Int(position.X, position.Y, 0);
                previewTilemap.SetTile(tilePosition, previewTile);
                previewTilemap.SetTileFlags(tilePosition, TileFlags.None);
                previewTilemap.SetColor(tilePosition, _spellPreviewTint);
            }
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




