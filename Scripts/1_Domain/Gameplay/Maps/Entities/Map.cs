using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.Entities
{
    /// <summary>
    /// タイル（マス）の集まりでできたマップです。
    /// </summary>
    public class Map
    {
        // マップの全タイルを持つ2次元配列です。
        private readonly Tile[,] _tiles;
        private readonly List<MapRect> _rooms = new List<MapRect>();
        private readonly ReadOnlyCollection<MapRect> _readOnlyRooms;

        /// <summary>
        /// マップの大きさ。
        /// </summary>
        public MapSize Size { get; }
        /// <summary>
        /// 部屋の一覧。
        /// </summary>
        public IReadOnlyList<MapRect> Rooms => _readOnlyRooms;
        /// <summary>
        /// スタート地点（最初に立つ場所）。
        /// </summary>
        public Position? StartPosition { get; private set; }
        /// <summary>
        /// 下り階段の場所。
        /// </summary>
        public Position? StairsDownPosition { get; private set; }

        /// <summary>
        /// マップを作るときの入口です。
        /// 最初は全部「壁」で埋めます。
        /// </summary>
        public Map(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            Size = new MapSize(width, height);
            _tiles = new Tile[width, height];
            _readOnlyRooms = _rooms.AsReadOnly();

            // まずは全部を壁にしておきます。
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new Tile(TileType.Wall);
                }
            }
        }

        /// <summary>
        /// 部屋の一覧を設定します。
        /// </summary>
        public void SetRooms(IEnumerable<MapRect> rooms)
        {
            _rooms.Clear();

            if (rooms == null)
            {
                return;
            }

            foreach (var room in rooms)
            {
                _rooms.Add(room);
            }
        }

        /// <summary>
        /// 指定位置が属する部屋を返します。
        /// </summary>
        public bool TryGetRoomAt(Position position, out MapRect room)
        {
            for (int i = 0; i < _rooms.Count; i++)
            {
                var candidate = _rooms[i];
                if (candidate.Contains(position))
                {
                    room = candidate;
                    return true;
                }
            }

            room = default;
            return false;
        }

        /// <summary>
        /// その位置がマップの中にあるかを調べます。
        /// </summary>
        public bool Contains(Position position) => Size.Contains(position);

        /// <summary>
        /// 指定の位置のタイルを取得します。
        /// マップの外なら壁扱いにします。
        /// </summary>
        public Tile GetTile(Position position)
        {
            if (!Contains(position))
            {
                return new Tile(TileType.Wall);
            }

            return _tiles[position.X, position.Y];
        }

        /// <summary>
        /// 指定の位置のタイルをセットします。
        /// </summary>
        public void SetTile(Position position, Tile tile)
        {
            if (!Contains(position))
            {
                return;
            }

            _tiles[position.X, position.Y] = tile;

            if (tile.Type == TileType.StairsDown)
            {
                StairsDownPosition = position;
            }
        }

        /// <summary>
        /// タイルの種類だけを変更します。
        /// </summary>
        public void SetTileType(Position position, TileType type)
        {
            var tile = GetTile(position);
            SetTile(position, tile.WithType(type));

            if (type == TileType.Floor && StartPosition == null)
            {
                StartPosition = position;
            }
        }

        /// <summary>
        /// スタート地点を設定します。
        /// マップ外ならエラーにします。
        /// </summary>
        public void SetStartPosition(Position position)
        {
            if (!Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Start position must be inside the map.");
            }

            if (!IsWalkable(position))
            {
                SetTileType(position, TileType.Floor);
            }

            StartPosition = position;
        }

        /// <summary>
        /// 下り階段の位置を設定します。
        /// </summary>
        public void SetStairsDownPosition(Position position)
        {
            if (!Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Stairs position must be inside the map.");
            }

            SetTileType(position, TileType.StairsDown);
            StairsDownPosition = position;
        }

        /// <summary>
        /// その位置が歩けるかどうか。
        /// </summary>
        public bool IsWalkable(Position position) => GetTile(position).IsWalkable;

        /// <summary>
        /// その位置が視界をふさぐかどうか。
        /// </summary>
        public bool BlocksSight(Position position) => GetTile(position).BlocksSight;

        /// <summary>
        /// いま見えているフラグをすべて消します。
        /// </summary>
        public void ClearVisibility()
        {
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    _tiles[x, y] = _tiles[x, y].WithVisibility(false);
                }
            }
        }

        /// <summary>
        /// 見えているマスの一覧をもとに、視界情報を更新します。
        /// </summary>
        public void ApplyVisibility(IReadOnlyCollection<Position> visiblePositions)
        {
            ClearVisibility();

            if (visiblePositions == null)
            {
                return;
            }

            foreach (var position in visiblePositions)
            {
                if (!Contains(position))
                {
                    continue;
                }

                _tiles[position.X, position.Y] = _tiles[position.X, position.Y].WithVisibility(true);
            }
        }

        /// <summary>
        /// 視界情報を更新します。
        /// 部屋にいる場合は部屋全体を可視にし、通路では周囲の正方形範囲を可視にします。
        /// </summary>
        public void ApplyVisibilityByRoomOrRadius(Position origin, int corridorRadius)
        {
            ClearVisibility();

            if (!TryGetRoomAt(origin, out var room))
            {
                var radius = Math.Max(0, corridorRadius);
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        var position = new Position(origin.X + dx, origin.Y + dy);
                        if (!Contains(position))
                        {
                            continue;
                        }

                        _tiles[position.X, position.Y] = _tiles[position.X, position.Y].WithVisibility(true);
                    }
                }

                return;
            }

            for (int x = room.Left; x <= room.Right; x++)
            {
                for (int y = room.Top; y <= room.Bottom; y++)
                {
                    _tiles[x, y] = _tiles[x, y].WithVisibility(true);
                }
            }
        }

        /// <summary>
        /// すべてのマスを可視・探索済みにします。
        /// </summary>
        public void RevealAll()
        {
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    _tiles[x, y] = _tiles[x, y].WithVisibility(true);
                }
            }
        }

        /// <summary>
        /// マップ内のすべての位置を順番に返します。
        /// </summary>
        public IEnumerable<Position> AllPositions()
        {
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    yield return new Position(x, y);
                }
            }
        }
    }
}



