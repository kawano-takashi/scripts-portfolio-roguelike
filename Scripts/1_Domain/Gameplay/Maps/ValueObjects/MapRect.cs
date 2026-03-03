using System;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.ValueObjects
{
    /// <summary>
    /// 四角い範囲（部屋など）を表す箱です。
    /// 左上の位置と幅・高さを持ちます。
    /// </summary>
    public readonly struct MapRect : IEquatable<MapRect>
    {
        /// <summary>
        /// 四角の左上X座標。
        /// </summary>
        public int X { get; }
        /// <summary>
        /// 四角の左上Y座標。
        /// </summary>
        public int Y { get; }
        /// <summary>
        /// 横の長さ。
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// 縦の長さ。
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 左端のX。
        /// </summary>
        public int Left => X;
        /// <summary>
        /// 右端のX。
        /// </summary>
        public int Right => X + Width - 1;
        /// <summary>
        /// 上端のY。
        /// </summary>
        public int Top => Y;
        /// <summary>
        /// 下端のY。
        /// </summary>
        public int Bottom => Y + Height - 1;

        /// <summary>
        /// 四角の中心位置（だいたい真ん中）です。
        /// </summary>
        public Position Center => new Position(X + Width / 2, Y + Height / 2);

        /// <summary>
        /// 四角を作るときの入口です。
        /// 幅・高さが0以下は許しません。
        /// </summary>
        public MapRect(int x, int y, int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// この四角の中に、その位置が入っているかを調べます。
        /// </summary>
        public bool Contains(Position position)
        {
            return position.X >= X && position.X < X + Width && position.Y >= Y && position.Y < Y + Height;
        }

        /// <summary>
        /// もう一つの四角と重なっているかを調べます。
        /// padding を入れると、少し広げて当たり判定します。
        /// </summary>
        public bool Intersects(MapRect other, int padding = 0)
        {
            var left = X - padding;
            var right = X + Width - 1 + padding;
            var top = Y - padding;
            var bottom = Y + Height - 1 + padding;

            return !(right < other.Left || left > other.Right || bottom < other.Top || top > other.Bottom);
        }

        /// <summary>
        /// 同じ四角かを比べます。
        /// </summary>
        public bool Equals(MapRect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        /// <summary>
        /// objectとして比べるときの処理です。
        /// </summary>
        public override bool Equals(object obj) => obj is MapRect other && Equals(other);

        /// <summary>
        /// ハッシュ用の数字を返します。
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        /// <summary>
        /// 文字で見える形にします。
        /// </summary>
        public override string ToString() => $"Rect({X},{Y},{Width},{Height})";
    }
}


