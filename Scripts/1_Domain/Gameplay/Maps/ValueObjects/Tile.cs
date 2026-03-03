using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.ValueObjects
{
    /// <summary>
    /// 1マス分の情報をまとめた箱です。
    /// </summary>
    public readonly struct Tile
    {
        /// <summary>
        /// そのマスの種類（壁・床など）。
        /// </summary>
        public TileType Type { get; }
        /// <summary>
        /// ここを「見たことがあるか」。
        /// </summary>
        public bool IsExplored { get; }
        /// <summary>
        /// いま「見えているか」。
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// タイルを作るときの入口です。
        /// </summary>
        public Tile(TileType type, bool isExplored = false, bool isVisible = false)
        {
            Type = type;
            IsExplored = isExplored;
            IsVisible = isVisible;
        }

        /// <summary>
        /// 歩いて通れるかどうか。
        /// </summary>
        public bool IsWalkable => Type == TileType.Floor || Type == TileType.DoorOpen || Type == TileType.StairsDown;

        /// <summary>
        /// 視界をふさぐかどうか。
        /// </summary>
        public bool BlocksSight => Type == TileType.Wall || Type == TileType.DoorClosed;

        /// <summary>
        /// 種類だけ変えた新しいタイルを返します。
        /// </summary>
        public Tile WithType(TileType type) => new Tile(type, IsExplored, IsVisible);

        /// <summary>
        /// 探索済みかどうかだけ変えた新しいタイルを返します。
        /// </summary>
        public Tile WithExplored(bool isExplored) => new Tile(Type, isExplored, IsVisible);

        /// <summary>
        /// 見えているかどうかを更新します。
        /// 見えたら探索済みにもします。
        /// </summary>
        public Tile WithVisibility(bool isVisible)
        {
            var explored = IsExplored || isVisible;
            return new Tile(Type, explored, isVisible);
        }
    }
}


