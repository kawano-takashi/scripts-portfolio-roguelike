using System;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.ValueObjects
{
    /// <summary>
    /// マップの「位置」を表す箱です。
    /// XとYの数字をセットで持ちます。
    /// </summary>
    public readonly struct Position
    {
        /// <summary>
        /// X座標（左右の位置）。
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Y座標（上下の位置）。
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// 原点（0,0）です。
        /// </summary>
        public static readonly Position Zero = new Position(0, 0);

        /// <summary>
        /// 位置を作るときの入口です。
        /// </summary>
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 位置どうしを足し算します。
        /// </summary>
        public Position Add(Position other) => new Position(X + other.X, Y + other.Y);

        /// <summary>
        /// 同じ位置かどうかを比べます。
        /// </summary>
        public override bool Equals(object obj) =>
            obj is Position other && X == other.X && Y == other.Y;

        /// <summary>
        /// ハッシュ用の数字を返します。
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// + で足し算できるようにします。
        /// </summary>
        public static Position operator +(Position a, Position b) => a.Add(b);

        /// <summary>
        /// == で比べられるようにします。
        /// </summary>
        public static bool operator ==(Position left, Position right) => left.Equals(right);

        /// <summary>
        /// != で比べられるようにします。
        /// </summary>
        public static bool operator !=(Position left, Position right) => !(left == right);

        /// <summary>
        /// 文字で見える形にします。
        /// </summary>
        public override string ToString() => $"({X}, {Y})";
    }
}


