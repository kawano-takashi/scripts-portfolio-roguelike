using System;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.ValueObjects
{
    /// <summary>
    /// マップの大きさ（横と縦）を持つ箱です。
    /// </summary>
    public readonly struct MapSize : IEquatable<MapSize>
    {
        /// <summary>
        /// 横の長さ（マスの数）。
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// 縦の長さ（マスの数）。
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 大きさを作るときの入口です。
        /// 0以下は許しません。
        /// </summary>
        public MapSize(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            Width = width;
            Height = height;
        }

        /// <summary>
        /// この大きさの中に、その位置が入っているかを調べます。
        /// </summary>
        public bool Contains(Position position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        /// <summary>
        /// 大きさが同じかを比べます。
        /// </summary>
        public bool Equals(MapSize other) => Width == other.Width && Height == other.Height;

        /// <summary>
        /// objectとして比べるときの処理です。
        /// </summary>
        public override bool Equals(object obj) => obj is MapSize other && Equals(other);

        /// <summary>
        /// ハッシュ用の数字を返します。
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(Width, Height);

        /// <summary>
        /// 文字で見える形にします。
        /// </summary>
        public override string ToString() => $"{Width}x{Height}";
    }
}


